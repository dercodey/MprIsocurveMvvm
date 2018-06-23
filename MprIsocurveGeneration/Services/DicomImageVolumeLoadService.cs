using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;

using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.LUT;
using Dicom.Imaging.Render;
using Dicom.Media;

using MprIsocurveGeneration.Models;
using System.IO;

namespace MprIsocurveGeneration.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class DicomImageVolumeLoadService : IDicomImageVolumeLoadService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public IEnumerable<DicomImageVolumeLoadStatus> 
            LoadUniformImageVolumeFromDicom(string directory, 
                Func<int, int, int, UniformImageVolumeModel> allocator)
        {
            var dcmFileNames = Directory.EnumerateFiles(directory, "*.dcm").ToList();
            var dcmFiles = from name in dcmFileNames select DicomFile.Open(name, Encoding.Default);
            var dcmImages = from file in dcmFiles select new DicomImage(file.Dataset);

            int slice = 0;
            List<DicomImage> allImages = new List<DicomImage>();
            foreach (var image in dcmImages)
            {
                allImages.Add(image);
                yield return new DicomImageVolumeLoadStatus()
                    {
                        Progress = 30 * slice++ / dcmFileNames.Count()
                    };
            }

            var width = allImages.Max(di => di.Width);
            var height = allImages.Max(di => di.Height);
            var depth = allImages.Count();

            Func<DicomImage, decimal> getSliceLocation =
                di => -di.Dataset.Get<decimal>(DicomTag.SliceLocation);

            slice = 0;
            List<int[]> allPixelsSorted = new List<int[]>();
            foreach (var image in allImages.OrderBy(getSliceLocation))
            {
                allPixelsSorted.Add(GetIntPixels(image));
                yield return new DicomImageVolumeLoadStatus()
                    {
                        Progress = 30 + 30 * slice++ / allImages.Count()
                    };
            }

#if USE_DICOM_WINDOW_LEVEL
            int windowCenter = (int) allImages.First().Dataset.Get<decimal>(DicomTag.WindowCenter);
            int windowWidth = (int) allImages.First().Dataset.Get<decimal>(DicomTag.WindowWidth);

            int minVoxelValue = windowCenter - windowWidth / 2;
            int maxVoxelValue = windowCenter + windowWidth / 2;
#else
            int minVoxelValue = allPixelsSorted.Min(pixels => pixels.Min());
            int maxVoxelValue = allPixelsSorted.Max(pixels => pixels.Max());
#endif
            slice = 0;
            var imageVolume = allocator(width, height, depth);
            foreach (var intPixels in allPixelsSorted)
            {
                lock (imageVolume)
                { 
                    for (int r = 0; r < imageVolume.Height; r++)
                    { 
                        for (int c = 0; c < imageVolume.Width; c++)
                        {
                            var value = intPixels[r * imageVolume.Width + c];
                            var wlValue = 255 * (value - minVoxelValue) 
                                / (maxVoxelValue - minVoxelValue);
                            imageVolume.Voxels[slice, r, c] =
                                (byte) Math.Max(0, Math.Min(255, wlValue));
                        }
                    }
                    imageVolume.SlicesCompleted++;
                }
                Thread.Sleep(250);
                yield return 
                    new DicomImageVolumeLoadStatus()
                    {
                        Progress = 60 + 30 * slice++ / allImages.Count(),
                        Done = false,
                        ImageVolume = imageVolume
                    };
            }

            yield return
                new DicomImageVolumeLoadStatus()
                {
                    Progress = 100,
                    Done = true,
                    ImageVolume = imageVolume
                };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private static int[] GetIntPixels(DicomImage image)
        {
            var gro = GrayscaleRenderOptions.FromDataset(image.Dataset);
            var voilut = VOILUT.Create(gro);

            var ipd = PixelDataFactory.Create(image.PixelData, 0);
            int[] outPixelsInt = new int[image.Width * image.Height];
            ipd.Render(voilut, outPixelsInt);
            return outPixelsInt;
        }
    }

    public class DicomImageVolumeLoadStatus
    {
        public int Progress;
        public bool Done;
        public UniformImageVolumeModel ImageVolume;
    }
}
