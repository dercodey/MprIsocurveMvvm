using System;
using System.Collections.Generic;

using Infrastructure.Interfaces;

namespace DataLoaderModule.Interfaces
{

    /// <summary>
    /// status struct to tell us about the status of a generation request
    /// </summary>
    public struct CreateUniformImageVolumeStatus
    {
        public int Progress;
        public bool Done;
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IGaussianVolumeFunction
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageVolume"></param>
        /// <returns></returns>
        IEnumerable<CreateUniformImageVolumeStatus> 
            PopulateGaussian(IUniformImageVolumeModel imageVolume);
    }
}
