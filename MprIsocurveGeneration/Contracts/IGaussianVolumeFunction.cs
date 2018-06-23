using System;
using System.Collections.Generic;

using MprIsocurveGeneration.Models;

namespace MprIsocurveGeneration.Services
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
            PopulateGaussian(UniformImageVolumeModel imageVolume);
    }
}
