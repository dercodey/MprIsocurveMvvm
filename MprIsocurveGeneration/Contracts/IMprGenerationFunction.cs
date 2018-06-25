using System.Threading.Tasks;

using DataLoaderModule.Models;
using MprIsocurveGeneration.Models;

namespace MprIsocurveGeneration.Services
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
            GenerateMprAsync(UniformImageVolumeModel inputVolume, 
                MprImageModel outImage);
    }
}
