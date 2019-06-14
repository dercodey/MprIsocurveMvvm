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
    abstract member GenerateMprAsync : inputVolume:IUniformImageVolumeModel * outImage:MprImageModelBase -> Task<byte[,]>
and MprImageModelBase(mprFunction:IMprGenerationFunction) = 
    member val InputVolumeGuid : Guid = Guid.Empty with get, set
    member val InputVolume : IUniformImageVolumeModel = null with get, set
    member val MprOrientation : Orientation = Orientation.Transverse with get, set
    member val SlicePosition : int = 0 with get, set

type IRenderedObject = 
    abstract member UpdateRenderedObject : orientation:Orientation * nSliceNumber:int -> Task<Action>
