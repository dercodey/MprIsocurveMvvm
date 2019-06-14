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


type MprGenerationFunction() =

    let calculateSize (uiv:IUniformImageVolumeModel) (orientation:Orientation) =
        match orientation with
        | Orientation.Transverse -> (uiv.Width, uiv.Height)
        | Orientation.Coronal -> (uiv.Width, uiv.Depth)
        | Orientation.Sagittal -> (uiv.Height, uiv.Depth)

    let updatePixelsFromVolume (uiv:IUniformImageVolumeModel) (orientation:Orientation) (slice:int) =
        let startTime : DateTime = DateTime.Now
        let msecStamp = (DateTime.Now - startTime).Milliseconds
        System.Diagnostics.Trace.WriteLine(
                String.Format("{0:D8}: UpdatePixelsFromVolume {1} {2} {3} @ slice {4} and orientation = {5}",
                    msecStamp, uiv.Width, uiv.Height, uiv.Depth, slice, orientation));

        // check for bounds based on the orientation
        match orientation with
        | Orientation.Transverse ->
            let slice = slice + uiv.Depth / 2 in 
                if (slice < 0 || slice >= uiv.Depth) then None else Some(slice)
        | Orientation.Coronal ->
            let slice = slice + uiv.Height / 2 in
                if (slice < 0 || slice >= uiv.Height) then None else Some(slice)
        | Orientation.Sagittal ->
            let slice = slice + uiv.Width / 2 in 
                if (slice < 0 || slice >= uiv.Width) then None else Some(slice)
        |> Option.map
            (fun slice -> 
                lock (uiv) 
                    (fun () -> 
                        let (width, height) = calculateSize uiv orientation
                        // now update based on the orientation
                        match orientation with
                        | Orientation.Transverse ->
                            Array2D.init width height (fun x y -> uiv.Voxels.[slice, y, x])
                        | Orientation.Coronal ->
                            Array2D.init width height (fun x y -> uiv.Voxels.[y, slice, x])
                        | Orientation.Sagittal ->
                            Array2D.init width height (fun x y -> uiv.Voxels.[y, x, slice])))
 
    interface IMprGenerationFunction with 
        member this.GenerateMprAsync(inputVolume:IUniformImageVolumeModel, outImage:MprImageModelBase, nSlicePosition:int) =
            let (width, height) = calculateSize inputVolume outImage.MprOrientation
            async { 
                let pixelsOption = updatePixelsFromVolume inputVolume outImage.MprOrientation outImage.SlicePosition
                match pixelsOption with
                | Some pixels -> return pixels 
                | None -> return null
            }  |> Async.StartAsTask


type IRenderedObject = 
    abstract member UpdateRenderedObject : orientation:Orientation * nSliceNumber:int -> Task<Action>
