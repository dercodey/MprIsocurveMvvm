namespace FsRenderModule.Services

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open System.Windows
open Infrastructure.Interfaces
open FsRenderModule.Interfaces


type IsocurveFunction() =

    let createOrAddSegment (geometry:ComplexGeometry)
            (segments:Dictionary<Point, LineSegments>)
            (x:int) (y:int) (startOffset:Vector) (endOffset:Vector) =
        let ptMiddle = Point(float x, float y)
        let startPoint = ptMiddle + startOffset
        let endPoint = ptMiddle + endOffset

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

    let addSegment (pixels:byte[,]) (geometry:ComplexGeometry) segments threshold (y,x) =

        let height = pixels.GetLength(0)
        let width = pixels.GetLength(1)

        // UL---UR    Bit order:
        //  |/ \|     (UL) (UR) (LR) (LL)
        //  |\ /|
        // LL---LR
        let ul = pixels.[y + height / 2, x + width / 2]
        let ur = pixels.[y + height / 2, x + width / 2 + 1]
        let lr = pixels.[y + height / 2 + 1, x + width / 2 + 1]
        let ll = pixels.[y + height / 2 + 1, x + width / 2]

        let byteThreshold = byte threshold

        let index =
            ((if (ul < byteThreshold) then 0 else 1) <<< 3)     // 1000
            ||| ((if (ur < byteThreshold) then 0 else 1) <<< 2) // 0100
            ||| ((if (lr < byteThreshold) then 0 else 1) <<< 1) // 0010
            ||| ((if (ll < byteThreshold) then 0 else 1) <<< 0) // 0001

        System.Diagnostics.Trace.Assert(index <= 0xF)
        System.Diagnostics.Trace.Assert(index >= 0x0)

        lock (geometry)
            (fun () -> 
                match index with
                | 0b0001|0b1110 ->                  
                    createOrAddSegment geometry  // UL---UR 
                        segments x y             //  |   |
                        (Vector(-0.5, 0.0))      //  |\  |
                        (Vector(0.0, 0.5))       // LL---LR
                | 0b0010|0b1101 ->
                    createOrAddSegment geometry  // UL---UR
                        segments x y             //  |   |
                        (Vector(0.0, 0.5))       //  |  /|
                        (Vector(0.5, 0.0))       // LL---LR
                | 0b0011|0b1100 ->
                    createOrAddSegment geometry  // UL---UR
                        segments x y             //  |___|
                        (Vector(-0.5, 0.0))      //  |   |
                        (Vector(0.5, 0.0))       // LL---LR
                | 0b0100|0b1011 ->
                    createOrAddSegment geometry  // UL---UR
                        segments x y             //  |  \|
                        (Vector(0.0, -0.5))      //  |   |
                        (Vector(0.5, 0.0))       // LL---LR
                | 0b0101|0b1010 ->
                    createOrAddSegment geometry  // UL---UR
                        segments x y             //  |  \|
                        (Vector(0.0, -0.5))      //  |\  |
                        (Vector(0.5, 0.0))       // LL---LR
                    createOrAddSegment geometry 
                        segments x y
                        (Vector(-0.5, 0.0))
                        (Vector(0.0, 0.5))
                | 0b0110|0b1001 ->
                    createOrAddSegment geometry  // UL---UR
                        segments x y             //  | | |
                        (Vector(0.0, -0.5))      //  | | |
                        (Vector(0.0, 0.5))       // LL---LR
                | 0b0111|0b1000 ->
                    createOrAddSegment geometry  // UL---UR 
                        segments x y             //  |/  |
                        (Vector(-0.5, 0.0))      //  |   |   
                        (Vector(0.0, -0.5))      // LL---LR
                | 0b1111|0b0000 -> ())

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
