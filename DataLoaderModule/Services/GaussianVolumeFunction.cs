using System;
using System.Collections.Generic;

using DataLoaderModule.Interfaces;
using DataLoaderModule.Models;

namespace DataLoaderModule.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class GaussianVolumeFunction : IGaussianVolumeFunction
    {
        /// <summary>
        /// generates a uniform volume populated with a gaussian distribution
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <returns>status enumerable that can be "pulled" to generate the volume</returns>
        public IEnumerable<CreateUniformImageVolumeStatus> 
            PopulateGaussian(UniformImageVolumeModel imageVolume)
        {
            int depth = imageVolume.Voxels.GetLength(0);
            int height = imageVolume.Voxels.GetLength(1);
            int width = imageVolume.Voxels.GetLength(2);

            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        double value = 0.0;
                        value += (depth / 2 - z) * (depth / 2 - z) / Math.Pow(depth / 2, 1.5) * 0.5;
                        value += (height / 2 - y) * (height / 2 - y) / Math.Pow(height / 2, 1.5) * 1.0;
                        value += (width / 2 - x) * (width / 2 - x) / Math.Pow(width / 2, 1.5) * 2.0;

                        imageVolume.Voxels[z, y, x] = (byte)(255.0 * Math.Exp(-value));
                    }

                    yield return new CreateUniformImageVolumeStatus()
                    {
                        Progress = 100 * (z * height + y) / (depth * height),
                        Done = false
                    };
                }
            }

            yield return new CreateUniformImageVolumeStatus()
            {
                Progress = 100,
                Done = true
            };
        }
    }
}
