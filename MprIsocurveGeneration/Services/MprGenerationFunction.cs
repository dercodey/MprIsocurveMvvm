using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;

using MprIsocurveGeneration.Models;

namespace MprIsocurveGeneration.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class MprGenerationFunction : IMprGenerationFunction
    {
        /// <summary>
        /// Generates an MPR for the uniform volume
        /// </summary>
        /// <seealso cref="CalculateSize"/> 
        /// <seealso cref="UpdatePixelFromVolume"/>
        /// <param name="inputVolume"></param>
        /// <param name="outImage"></param>
        /// <returns>task with byte array future</returns>
        public async Task<byte[,]> 
            GenerateMprAsync(UniformImageVolumeModel inputVolume, MprImageModel outImage)
        {
            int width, height;
            CalculateSize(inputVolume, outImage.MprOrientation, out width, out height);

            return await 
                Task.Run<byte[,]>(() =>
                {
                    var pixels = new byte[height, width];
                    UpdatePixelsFromVolume(inputVolume, outImage.MprOrientation,
                        outImage.SlicePosition, ref pixels);
                    return pixels;
                });
        }

        /// <summary>
        /// helper to calculate the needed size for the MPR, based on 
        /// </summary>
        /// <seealso cref="UniformImageVolumeModel.Width"/> 
        /// <seealso cref="UniformImageVolumeModel.Height"/> 
        /// <seealso cref="UniformImageVolumeModel.Depth"/> 
        /// <seealso cref="MprImageModel.Orientation"/> 
        /// <param name="uiv"></param>
        /// <param name="orientation"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        static void CalculateSize(UniformImageVolumeModel uiv, MprImageModel.Orientation orientation, 
            out int width, out int height)
        {
            width = 0;
            height = 0;
            switch (orientation)
            {
                case MprImageModel.Orientation.Transverse:
                    width = uiv.Width;
                    height = uiv.Height;
                    break;

                case MprImageModel.Orientation.Coronal:
                    width = uiv.Width;
                    height = uiv.Depth;
                    break;

                case MprImageModel.Orientation.Sagittal:
                    width = uiv.Height;
                    height = uiv.Depth;
                    break;
            }
        }

        // helper to time-stamp the log messages
        static DateTime startTime = DateTime.Now;

        /// <summary>
        /// helper to calculate the update pixel values
        /// </summary>
        /// <seealso cref="UniformImageVolumeModel.Width"/> 
        /// <seealso cref="UniformImageVolumeModel.Height"/> 
        /// <seealso cref="UniformImageVolumeModel.Depth"/> 
        /// <seealso cref="MprImageModel.Orientation"/> 
        /// <param name="uiv"></param>
        /// <param name="orientation"></param>
        /// <param name="slice"></param>
        /// <param name="pixels"></param>
        static void UpdatePixelsFromVolume(UniformImageVolumeModel uiv,
            MprImageModel.Orientation orientation, int slice, ref byte[,] pixels)
        {
            // create a log to debug output
            var msecStamp = (DateTime.Now - startTime).Milliseconds;
            System.Diagnostics.Trace.WriteLine(
                String.Format("{0:D8}: UpdatePixelsFromVolume {1} {2} {3} @ slice {4} and orientation = {5}",
                msecStamp, uiv.Width, uiv.Height, uiv.Depth, slice, orientation));

            // capture stack trace here
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();

            // check for bounds based on the orientation
            switch (orientation)
            {
                case MprImageModel.Orientation.Transverse:
                    slice += uiv.Depth / 2;
                    if (slice < 0 || slice >= uiv.Depth)
                        return;
                    break;

                case MprImageModel.Orientation.Coronal:
                    slice += uiv.Height / 2;
                    if (slice < 0 || slice >= uiv.Height)
                        return;
                    break;

                case MprImageModel.Orientation.Sagittal:
                    slice += uiv.Width / 2;
                    if (slice < 0 || slice >= uiv.Width)
                        return;
                    break;
            }

            lock (uiv)
            { 
                // now update based on the orientation
                switch (orientation)
                {
                    case MprImageModel.Orientation.Transverse:
                        for (int y = 0; y < pixels.GetLength(0); y++)
                            for (int x = 0; x < pixels.GetLength(1); x++)
                                pixels[y, x] = uiv.Voxels[slice, y, x];
                        break;

                    case MprImageModel.Orientation.Coronal:
                        for (int y = 0; y < pixels.GetLength(0); y++)
                            for (int x = 0; x < pixels.GetLength(1); x++) 
                                pixels[y, x] = uiv.Voxels[y, slice, x];
                        break;

                    case MprImageModel.Orientation.Sagittal:
                        for (int y = 0; y < pixels.GetLength(0); y++)
                            for (int x = 0; x < pixels.GetLength(1); x++) 
                                pixels[y, x] = uiv.Voxels[y, x, slice];
                        break;
                }
            }
        }
    }
}
