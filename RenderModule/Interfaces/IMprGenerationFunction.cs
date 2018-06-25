using System.Threading.Tasks;

using Infrastructure.Interfaces;
using RenderModule.Models;

namespace RenderModule.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMprGenerationFunction
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputVolume"></param>
        /// <param name="outImage"></param>
        /// <returns></returns>
        Task<byte[,]> 
            GenerateMprAsync(IUniformImageVolumeModel inputVolume, 
                MprImageModel outImage);
    }
}
