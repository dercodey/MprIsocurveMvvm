﻿namespace FsRenderModule.Services

open System
open FsRenderModule.Interfaces
open Infrastructure.Interfaces

type MprGenerationFunction() =

    let updatePixelsFromVolume (uiv:IUniformImageVolumeModel) (orientation:Orientation) (origSlice:int) =
        let startTime : DateTime = DateTime.Now
        let msecStamp = (DateTime.Now - startTime).Milliseconds
        System.Diagnostics.Trace.WriteLine(
                String.Format("{0:D8}: UpdatePixelsFromVolume {1} {2} {3} @ slice {4} and orientation = {5}",
                    msecStamp, uiv.Width, uiv.Height, uiv.Depth, origSlice, orientation));

        // check for bounds based on the orientation
        let width, height, size, initFunc = 
            match orientation with
            | Orientation.Transverse -> 
                uiv.Width, uiv.Height, uiv.Depth, (fun slice x y -> uiv.Voxels.[slice, y, x])
            | Orientation.Coronal -> 
                uiv.Width, uiv.Depth, uiv.Height, (fun slice x y -> uiv.Voxels.[y, slice, x])
            | Orientation.Sagittal -> 
                uiv.Height, uiv.Depth, uiv.Width, (fun slice x y -> uiv.Voxels.[y, x, slice])
        
        let slice = origSlice + size/2
        if 0 <= slice && slice < size
        then 
            lock (uiv) (fun () -> Some(Array2D.init width height (initFunc slice)))
        else
            None

    interface IMprGenerationFunction with 
        member this.GenerateMprAsync(inputVolume:IUniformImageVolumeModel, outImage:MprImageModelBase, nSlicePosition:int) =
            async { 
                let pixelsOption = updatePixelsFromVolume inputVolume outImage.MprOrientation outImage.SlicePosition
                match pixelsOption with
                | Some pixels -> return pixels 
                | None -> return null
            }  |> Async.StartAsTask
