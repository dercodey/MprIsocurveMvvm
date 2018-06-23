using System;
using MprIsocurveGeneration.Models;

namespace MprIsocurveGeneration.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IModelRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputVolumeGuid"></param>
        /// <returns></returns>
        Guid CreateMprImage(Guid inputVolumeGuid);

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
        MprImageModel GetMprImage(Guid guid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        UniformImageVolumeModel GetUniformImageVolume(Guid guid);
    }
}
