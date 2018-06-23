using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MprIsocurveGeneration.Models;

namespace MprIsocurveGeneration.ViewModels
{
    using Orientation = PresentationStateViewModel.Orientation;

    /// <summary>
    /// interface to be supported by VMs that are rendered objects
    /// </summary>
    public interface IRenderedObjectViewModel
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
