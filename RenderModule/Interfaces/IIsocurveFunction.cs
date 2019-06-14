using System;
using System.Threading.Tasks;
using FsRenderModule.Interfaces;
using RenderModule.Models;

namespace RenderModule.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IIsocurveFunction
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromImage"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        Task<ComplexGeometry> 
            GenerateIsocurveAsync(MprImageModelBase fromImage, float threshold);
    }
}
