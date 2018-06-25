using System;
using System.Threading.Tasks;

using static RenderModule.Models.MprImageModel;

namespace RenderModule.Interfaces
{
    /// <summary>
    /// interface to be supported by VMs that are rendered objects
    /// </summary>
    public interface IRenderedObject
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="orientation"></param>
        /// <param name="nSliceNumber"></param>
        /// <param name="uiUpdateAction"></param>
        Task<Action> UpdateRenderedObject(Orientation orientation, int nSliceNumber);
    }
}
