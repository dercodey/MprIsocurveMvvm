using System;
using System.Collections.Generic;

using MprIsocurveGeneration.Models;

namespace MprIsocurveGeneration.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDicomImageVolumeLoadService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="allocator"></param>
        /// <returns></returns>
        IEnumerable<DicomImageVolumeLoadStatus> 
            LoadUniformImageVolumeFromDicom(string directory, 
                Func<int, int, int, UniformImageVolumeModel> allocator);
    }
}
