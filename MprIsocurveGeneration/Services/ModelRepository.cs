using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using MprIsocurveGeneration.Models;
using Microsoft.Practices.Unity;

namespace MprIsocurveGeneration.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class ModelRepository : IModelRepository
    {
        IUnityContainer _container;

        ConcurrentDictionary<Guid, MprImageModel> _mprImages =
            new ConcurrentDictionary<Guid, MprImageModel>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        public ModelRepository(IUnityContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputVolumeGuid"></param>
        /// <returns></returns>
        public Guid CreateMprImage(Guid inputVolumeGuid)
        {
            var guid = Guid.NewGuid();
            var mprImageModel = _container.Resolve<MprImageModel>();
            mprImageModel.InputVolumeGuid = inputVolumeGuid;
            mprImageModel.InputVolume = _imageVolumes[inputVolumeGuid];
            bool bSuccess = _mprImages.TryAdd(guid, mprImageModel);
            System.Diagnostics.Trace.Assert(bSuccess);
            return guid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public MprImageModel GetMprImage(Guid guid)
        {
            return _mprImages[guid];
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
        public UniformImageVolumeModel GetUniformImageVolume(Guid guid)
        {
            return _imageVolumes[guid];
        }
    }
}
