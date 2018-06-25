using System;
using System.Threading.Tasks;

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
            GenerateIsocurveAsync(MprImageModel fromImage, float threshold);
    }
}
