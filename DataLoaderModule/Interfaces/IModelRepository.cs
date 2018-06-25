using System;

using DataLoaderModule.Models;

namespace DataLoaderModule.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IModelRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        Guid CreateUniformImageVolume(int width, int height, int depth);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        UniformImageVolumeModel GetUniformImageVolume(Guid guid);
    }
}
