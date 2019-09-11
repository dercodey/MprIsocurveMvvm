namespace FsRenderModule.Services

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open System.Windows
open Infrastructure.Interfaces
open FsRenderModule.Interfaces

type Edge =
| Left of int * int
| Top of int * int

type Segment = Edge * Edge
type SegmentPath = list<Segment>

module TestCompute = 
    let edgeCompute (pixels:byte[,]) (thresh:byte) =
        let edgesFromPixels (x,y) =
            let ul, ur, ll, lr = 
                pixels.[x,y] < thresh,
                pixels.[x,y+1] < thresh,
                pixels.[x,y] < thresh,
                pixels.[x,y+1] < thresh

            match ul, ur, ll, lr with
            | true, true, true, true
            | false, false, false, false -> 
                [ ]

            | true, true, true, false           // ul---ur 
            | false, false, false, true ->      //  |\  |
                [(Left(x,y), Top(x,y+1))]       // ll---lr

            | true, true, false, true           // ul---ur
            | false, false, true, false ->      //  |  /|
                [(Left(x+1,y), Top(x,y+1))]     // ll---lr

            | true, true, false, false          // ul---ur
            | false, false, true, true ->       //  |---|
                [(Left(x,y), Left(x+1,y))]      // ll---lr

            | true, false, true, true           // ul---ur
            | false, true, false, false ->      //  |  \|
                [(Top(x,y), Left(x+1,y))]       // ll---lr

            | true, false, true, false          // ul---ur
            | false, true, false, true ->       //  |\ \|
                [(Left(x,y), Top(x,y+1));       // ll---lr
                   (Top(x,y), Left(x+1,y))]

            | true, false, false, true          // ul---ur
            | false, true, true, false ->       //  | | |
                [(Top(x,y), Top(x,y+1))]        // ll---lr

            | true, false, false, false         // ul---ur
            | false, true, true, true ->        //  |/  |
                [(Left(x,y), Top(x,y))]         // ll---lr

        let updateCache (cacheMap:Map<Edge, SegmentPath>) (segment:Segment) =
            let (startEdge, endEdge) = segment
            cacheMap
            |> Map.partition (fun key _ -> key = startEdge||key = endEdge)        
            |> function
                matchMap, restMap ->
                    match matchMap.ContainsKey(startEdge), 
                            matchMap.ContainsKey(endEdge) with
                    | true, true ->
                        restMap
                        |> Map.add startEdge ((startEdge,endEdge) :: matchMap.[endEdge])
                        |> Map.add endEdge ((endEdge,startEdge) :: matchMap.[startEdge])
                    | true, false ->
                        restMap
                        |> Map.add endEdge ((endEdge,startEdge) :: matchMap.[startEdge])
                    | false, true ->
                        restMap
                        |> Map.add startEdge ((startEdge, endEdge) :: matchMap.[endEdge])
                    | false, false ->
                        restMap
                        |> Map.add startEdge [(startEdge, endEdge)]
                        |> Map.add endEdge [(endEdge, startEdge)]

        (seq { 0..pixels.GetLength(0) }, 
            seq { 0..pixels.GetLength(1) })
        ||> Seq.allPairs
        |> Seq.map edgesFromPixels
        |> Seq.concat
        |> Seq.fold updateCache Map.empty<Edge, SegmentPath>
        |> function
            resultMap -> 
                resultMap

type IsocurveFunction() =

    let startEndForPixel x y startOffset endOffset = 
        let ptMiddle = Point(float x, float y)
        let startPoint = ptMiddle + startOffset
        let endPoint = ptMiddle + endOffset
        startPoint, endPoint

    let createOrAddSegment (geometry:ComplexGeometry)
            (segments:Dictionary<Point, LineSegments>)
            (x:int) (y:int) (startOffset:Vector) (endOffset:Vector) =
        let startPoint, endPoint = startEndForPixel x y startOffset endOffset

        if (segments.ContainsKey(startPoint))
        then 
            let pc = segments.[startPoint]
            segments.Remove(startPoint) |> ignore
            if (pc.First() = startPoint)
            then 
                pc.Insert(0, endPoint) |> ignore
                if not(segments.ContainsKey(endPoint))
                then segments.Add(endPoint, pc)
            elif (pc.Last() = startPoint)
            then
                (pc.Add(endPoint)) |> ignore
                if not(segments.ContainsKey(endPoint))
                then segments.Add(endPoint, pc)
        elif (segments.ContainsKey(endPoint))
        then 
            let pc = segments.[endPoint]
            segments.Remove(endPoint) |> ignore

            if (pc.First() = endPoint)
            then 
                pc.Insert(0, startPoint)
                if not(segments.ContainsKey(startPoint))
                then segments.Add(startPoint, pc)
            elif (pc.Last() = endPoint)
            then
                pc.Add(startPoint)
                if not(segments.ContainsKey(startPoint))
                then segments.Add(startPoint, pc)
        else
            let pc = new LineSegments()
            pc.Add(startPoint)
            pc.Add(endPoint)
            segments.Add(startPoint, pc)
            segments.Add(endPoint, pc)
            geometry.Add(pc)

    let createOrAddSegmentMap (geometry:ComplexGeometry)
            (segments:Map<decimal*decimal, LineSegments>)
            (x:int) (y:int) (startOffset:Vector) (endOffset:Vector) =
        let startPoint, endPoint = 
            startEndForPixel x y startOffset endOffset
            |> function 
                ox, oy -> (decimal ox.X, decimal ox.Y), 
                            (decimal oy.X, decimal oy.Y)
        segments
        |> Map.partition (fun key _ -> key = startPoint || key = endPoint)
        |> function
            matchMap, restMap ->
                match matchMap.ContainsKey(startPoint), 
                        matchMap.ContainsKey(endPoint) with
                | true, true ->
                    matchMap.[startPoint].Add(Point(0.,0.))
                    matchMap.[endPoint].Add(Point(0.,0.))
                    restMap
                | true, false ->
                    matchMap.[startPoint].Add(Point(0.,0.))
                    restMap
                    |> Map.add startPoint null
                | false, true ->
                    let segments = matchMap.[endPoint]
                    segments.Add(Point(0.,0.))
                    restMap
                    |> Map.add startPoint segments
                | false, false ->
                    restMap
                    |> Map.add startPoint null
                    |> Map.add endPoint null

    let addSegment (pixels:byte[,]) (geometry:ComplexGeometry) segments threshold (y,x) =

        let height = pixels.GetLength(0)
        let width = pixels.GetLength(1)

        // UL---UR    Corner order:
        //  |/ \|     (UL) (UR) (LR) (LL)
        //  |\ /|
        // LL---LR
        let ul = pixels.[y + height / 2, x + width / 2]
        let ur = pixels.[y + height / 2, x + width / 2 + 1]
        let lr = pixels.[y + height / 2 + 1, x + width / 2 + 1]
        let ll = pixels.[y + height / 2 + 1, x + width / 2]

        let bThresh = byte threshold

        // match index with
        match ul < bThresh, ur < bThresh, lr < bThresh, ll < bThresh with
        | false, false, false, true         // UL---UR 
        | true, true, true, false ->        //  |   |
            createOrAddSegment geometry     //  |\  |
                segments x y                // LL---LR
                (Vector(-0.5, 0.0))      
                (Vector(0.0, 0.5))       
        | false, false, true, false
        | true, true, false, true ->
            createOrAddSegment geometry  // UL---UR
                segments x y             //  |   |
                (Vector(0.0, 0.5))       //  |  /|
                (Vector(0.5, 0.0))       // LL---LR
        | false, false, true, true
        | true, true, false, false ->
            createOrAddSegment geometry  // UL---UR
                segments x y             //  |___|
                (Vector(-0.5, 0.0))      //  |   |
                (Vector(0.5, 0.0))       // LL---LR
        | false, true, false, false
        | true, false, true, true ->
            createOrAddSegment geometry  // UL---UR
                segments x y             //  |  \|
                (Vector(0.0, -0.5))      //  |   |
                (Vector(0.5, 0.0))       // LL---LR
        | false, true, false, true
        | true, false, true, false ->
            createOrAddSegment geometry  // UL---UR
                segments x y             //  |  \|
                (Vector(0.0, -0.5))      //  |\  |
                (Vector(0.5, 0.0))       // LL---LR
            createOrAddSegment geometry 
                segments x y
                (Vector(-0.5, 0.0))
                (Vector(0.0, 0.5))
        | false, true, true, false
        | true, false, false, true ->
            createOrAddSegment geometry  // UL---UR
                segments x y             //  | | |
                (Vector(0.0, -0.5))      //  | | |
                (Vector(0.0, 0.5))       // LL---LR
        | false, true, true, true
        | true, false, false, false ->
            createOrAddSegment geometry  // UL---UR 
                segments x y             //  |/  |
                (Vector(-0.5, 0.0))      //  |   |   
                (Vector(0.0, -0.5))      // LL---LR
        | true, true, true, true
        | false, false, false, false -> ()

    interface IIsocurveFunction with
        member this.GenerateIsocurveAsync(fromImage:MprImageModelBase, threshold:float) : Task<ComplexGeometry> =
            let pixels = fromImage.GetPixelsAsync().Result
            let geometry = ComplexGeometry()
            if (pixels <> null)
            then 
                // stores the dictionary of segments
                let segments = Dictionary<Point, LineSegments>()

                let height = pixels.GetLength(0)
                let width = pixels.GetLength(1)

                // Y is increasing as we move down the image
                //      Upper is -Y
                //      Lower is +Y
                (seq {-height/2..height/2 - 2},
                    seq {-width/2..width/2 - 2})
                ||> Seq.allPairs
                |> Seq.iter (addSegment pixels geometry segments threshold)
                Task.FromResult(geometry)
            else 
                Task.FromResult(geometry)
