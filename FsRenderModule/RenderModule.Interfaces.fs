namespace FsRenderModule.Interfaces

open System
open System.Threading
open System.Threading.Tasks
open Infrastructure.Interfaces

type Orientation =
| Transverse = 0
| Sagittal = 1
| Coronal = 2

type IMprGenerationFunction = 
    abstract member GenerateMprAsync : inputVolume:IUniformImageVolumeModel * outImage:MprImageModelBase * nSlicePosition:int -> Task<byte[,]>
and MprImageModelBase(mprFunction:IMprGenerationFunction) = 
    let mutable _pixelTask : Task<byte[,]> = null
    // these hold the current values being used for the current pixel task
    let mutable _taskMprOrientation : Orientation = Orientation.Transverse
    let mutable _taskSlicePosition : int = 0
    let mutable _volumeSlicesCompleted : int = 0
    member val InputVolumeGuid : Guid = Guid.Empty with get, set
    member val InputVolume : IUniformImageVolumeModel = null with get, set
    member val MprOrientation : Orientation = Orientation.Transverse with get, set
    member val SlicePosition : int = 0 with get, set
    member this.CheckAndUpdate(orientation:Orientation, nSliceNumber:int) =
        if (this.MprOrientation <> orientation
            || this.SlicePosition <> nSliceNumber)
            //|| _pixelTask = null
            //|| _volumeSlicesCompleted < this.InputVolume.SlicesCompleted)
        then
            this.MprOrientation <- orientation
            this.SlicePosition <- nSliceNumber
            true
        else false
    member this.GetPixelsAsync() =
        mprFunction.GenerateMprAsync(this.InputVolume, this, this.SlicePosition)
        //if (_pixelTask = null
        //    || this.MprOrientation.CompareTo(_taskMprOrientation) <> 0
        //    || this.SlicePosition <> _taskSlicePosition
        //    || _volumeSlicesCompleted <> this.InputVolume.SlicesCompleted)
        //then
        //    _taskMprOrientation <- this.MprOrientation
        //    _taskSlicePosition <- this.SlicePosition
        //    _volumeSlicesCompleted <- this.InputVolume.SlicesCompleted
        //    _pixelTask <- mprFunction.GenerateMprAsync(this.InputVolume, this)
        //    _pixelTask
        //else
        //    _pixelTask

type IRenderedObject = 
    abstract member UpdateRenderedObject : orientation:Orientation * nSliceNumber:int -> Task<Action>
