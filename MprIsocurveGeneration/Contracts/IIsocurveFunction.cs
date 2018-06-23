using System;
using System.Threading.Tasks;

using MprIsocurveGeneration.Models;

namespace MprIsocurveGeneration.Services
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
