using System;
using System.Collections.Concurrent;

using Microsoft.Practices.Unity;

using Infrastructure.Interfaces;
using DataLoaderModule.Interfaces;
using DataLoaderModule.Models;

namespace DataLoaderModule.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class ModelRepository : IModelRepository
    {
        IUnityContainer _container;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        public ModelRepository(IUnityContainer container)
        {
            _container = container;
        }

        // internal map to retrieve image volumes by GUID
        ConcurrentDictionary<Guid, UniformImageVolumeModel> _imageVolumes =
            new ConcurrentDictionary<Guid, UniformImageVolumeModel>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public Guid CreateUniformImageVolume(int width, int height, int depth)
        {
            var uiv = new UniformImageVolumeModel();
            uiv.Voxels = new byte[depth, height, width];
            var guid = Guid.NewGuid();
            bool bSuccess = _imageVolumes.TryAdd(guid, uiv);
            System.Diagnostics.Trace.Assert(bSuccess);
            return guid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public IUniformImageVolumeModel GetUniformImageVolume(Guid guid)
        {
            return _imageVolumes[guid];
        }
    }
}
