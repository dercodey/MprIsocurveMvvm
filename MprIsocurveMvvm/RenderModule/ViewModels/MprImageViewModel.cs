using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Prism.Mvvm;

using Infrastructure.Utilities;
using Infrastructure.Interfaces;
using FsRenderModule.Interfaces;

namespace RenderModule.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class MprImageViewModel : FsRenderModule.ViewModels.MprImageViewModel
    {
        public MprImageViewModel(IModelRepository repository, IMprGenerationFunction mprGenerator)
            : base(repository, mprGenerator)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="counter"></param>
        public void SetPerformanceCounter(PerformanceCounter counter)
        {
            _counter = counter;
        }
        PerformanceCounter _counter;
    }
}
