using System;

using Infrastructure.Interfaces;

namespace Infrastructure.Interfaces
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
        IUniformImageVolumeModel GetUniformImageVolume(Guid guid);
    }
}
