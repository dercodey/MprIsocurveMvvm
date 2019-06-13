namespace FsRenderModule.Interfaces

open System
open System.Threading
open System.Threading.Tasks
open Infrastructure.Interfaces

type Orientation =
| Transverse
| Sagittal
| Coronal

type IMprGenerationFunction = 
    abstract member GenerateMprAsync : inputVolume:IUniformImageVolumeModel * outImage:MprImageModel -> Task<byte[,]>
and MprImageModel(mprFunction:IMprGenerationFunction) = 
    member val InputVolumeGuid : Guid = Guid.Empty
    member val InputVolume : IUniformImageVolumeModel = 
        { new IUniformImageVolumeModel with 
            member this.Height = 0 
            member this.Width = 0 
            member this.Depth = 0 
            member this.Voxels with get() = Array3D.zeroCreate 1 1 1 and set value = ()
            member this.SlicesCompleted with get() = 0 and set value = ()}
    member val MprOrientation : Orientation = Orientation.Coronal
    member val SlicePosition : int = 0


type Point = class end
type Vector = class end
type LineSegments = List<Point>

type ComplexGeometry() = class end
    //inherit List<LineSegments>()
    //let _segments = new Dictionary<Point, LineSegments>()

    //member this.CreateOrAddSegment(x:int, y:int, startOffset:Vector, endOffset:Vector) =
    //    Point ptMiddle = new Point(x, y)
    //    Point startPoint = ptMiddle + startOffset
    //    Point endPoint = ptMiddle + endOffset

    //    if (_segments.ContainsKey(startPoint))
    //    then 
    //        let pc = _segments[startPoint]
    //        _segments.Remove(startPoint)

    //        if (pc.First() = startPoint)
    //        then
    //            pc.Insert(0, endPoint)
    //        else if (pc.Last() = startPoint)
    //            pc.Add(endPoint)

    //        if (!_segments.ContainsKey(endPoint))
    //        then 
    //            _segments.Add(endPoint, pc)

    //    else if (_segments.ContainsKey(endPoint))
    //        let pc = _segments[endPoint]
    //        _segments.Remove(endPoint)

    //        if (pc.First() = endPoint)
    //        then
    //            pc.Insert(0, startPoint);
    //        else if (pc.Last() = endPoint)
    //            pc.Add(startPoint);

    //        if (!_segments.ContainsKey(startPoint))
    //        then
    //            _segments.Add(startPoint, pc);
    //    else
    //        let pc = new LineSegments()
    //        pc.Add(startPoint)
    //        pc.Add(endPoint)
    //        _segments.Add(startPoint, pc);
    //        _segments.Add(endPoint, pc);

    //        this.Add(pc);


type IIsocurveFunction =
    abstract member GenerateIsocurveAsync : fromImage:MprImageModel * threshold:float -> Task<ComplexGeometry>

type IRenderedObject = 
    abstract member UpdateRenderedObject : orientation:Orientation * nSliceNumber:int -> Task<Action>

type PerformanceCounter = class end

type IFrameUpdateManager = 
    abstract member QueueAction : timeStamp:UInt64*context:SynchronizationContext*action:Task<Action> -> unit
    abstract member ProcessQueue : unit -> unit
    abstract member UiUpdatePerformance : PerformanceCounter
