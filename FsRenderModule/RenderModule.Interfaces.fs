namespace FsRenderModule.Interfaces

open System
open System.Threading
open System.Threading.Tasks
open Infrastructure.Interfaces
open System.Windows

type Orientation =
| Transverse = 0
| Sagittal = 1
| Coronal = 2

type IMprGenerationFunction = 
    abstract member GenerateMprAsync : inputVolume:IUniformImageVolumeModel * orientation:Orientation * nSlicePosition:int -> Task<byte[,]>
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
        mprFunction.GenerateMprAsync(this.InputVolume, this.MprOrientation, this.SlicePosition)

type LineSegments = System.Collections.Generic.List<Point>
type ComplexGeometry = System.Collections.Generic.List<LineSegments>
type IIsocurveFunction = 
    abstract member GenerateIsocurveAsync : fromImage:MprImageModelBase * threshold:float -> Task<ComplexGeometry>

type IRenderedObject = 
    abstract member UpdateRenderedObject : orientation:Orientation * nSliceNumber:int -> Task<Action>
