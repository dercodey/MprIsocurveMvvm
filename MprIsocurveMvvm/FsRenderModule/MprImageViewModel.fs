namespace FsRenderModule.ViewModels

open System
open System.Threading.Tasks
open System.Windows.Media.Imaging
open System.Windows.Media
open Prism.Mvvm
open Infrastructure.Interfaces
open FsRenderModule.Interfaces

type MprImageViewModel(repository:IModelRepository, mprGenerator:IMprGenerationFunction) as this =
    inherit BindableBase()

    let updateImageSource (pixels:byte[,])  =
        let height, width = pixels.GetLength(0), pixels.GetLength(1)
        this.MprBuffer <- 
            let flatPixelAt n = pixels.[int (n/width), n % width]
            let flatPixels = Array.init<byte> (height * width) flatPixelAt
            BitmapSource.Create(width, height, 96.0, 96.0, PixelFormats.Gray8, null, flatPixels, width)
        this.ImagePosition <- new TranslateTransform(-float width / 2.0, -float height / 2.0)

    let mutable _mprBuffer : ImageSource = null
    let mutable _imagePosition : Transform = null

    member val VolumeGuid = Guid.Empty with get, set
    member this.MprBuffer
        with get() = _mprBuffer
        and set (value:ImageSource) = 
            base.SetProperty(ref _mprBuffer, value) |> ignore
    member this.ImagePosition
        with get() = _imagePosition
        and set (value:Transform) = 
            base.SetProperty(ref _imagePosition, value) |> ignore
    //member this.SetPerformanceCounter(counter:PerformanceCounter)
    //    _counter = counter;
    //PerformanceCounter _counter;

    interface IRenderedObject with
        member this.UpdateRenderedObject(orientation:Orientation, nSliceNumber:int) : Task<Action> =
            //_counter.StartEvent();
            let uiv = repository.GetUniformImageVolume(this.VolumeGuid);
            let pixels = mprGenerator.GenerateMprAsync(uiv, orientation, nSliceNumber).Result
            //_counter.EndEvent();

            // perform the update on the UI thread
            Task.FromResult<Action>(fun () -> if pixels <> null then updateImageSource(pixels) else ())
