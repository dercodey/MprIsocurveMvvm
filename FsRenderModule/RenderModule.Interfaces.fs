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
    member val InputVolumeGuid : Guid = Guid.Empty with get, set
    member val InputVolume : IUniformImageVolumeModel = null with get, set
    member val MprOrientation : Orientation = Orientation.Transverse with get, set
    member val SlicePosition : int = 0 with get, set
    member this.CheckAndUpdate(orientation:Orientation, nSliceNumber:int) =
        if (this.MprOrientation <> orientation
            || this.SlicePosition <> nSliceNumber)
        then
            this.MprOrientation <- orientation
            this.SlicePosition <- nSliceNumber
            true
        else false
    member this.GetPixelsAsync() =
        mprFunction.GenerateMprAsync(this.InputVolume, this, this.SlicePosition)

type IRenderedObject = 
    abstract member UpdateRenderedObject : orientation:Orientation * nSliceNumber:int -> Task<Action>
