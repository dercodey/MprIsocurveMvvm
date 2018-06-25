using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Prism.Mvvm;

using AutoMapper;

using Infrastructure.Utilities;

using DataLoaderModule.Interfaces;

using RenderModule.Models;

namespace MprIsocurveGeneration.ViewModels
{
    using Orientation = PresentationStateViewModel.Orientation;

    /// <summary>
    /// 
    /// </summary>
    public class MprImageViewModel : BindableBase, IRenderedObjectViewModel
    {
        IModelRepository _repository;

        public MprImageViewModel(IModelRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 
        /// </summary>
        public MprImageModel MprImageModel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid MprGuid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ImageSource MprBuffer
        {
            get { return _mprBuffer; }
            set { SetProperty(ref _mprBuffer, value); }
        }
        private ImageSource _mprBuffer;

        /// <summary>
        /// 
        /// </summary>
        public Transform ImagePosition
        {
            get { return _imagePosition; }
            set { SetProperty(ref _imagePosition, value); }
        }
        private Transform _imagePosition;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="counter"></param>
        public void SetPerformanceCounter(PerformanceCounter counter)
        {
            _counter = counter;
        }
        PerformanceCounter _counter;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="orientation"></param>
        /// <param name="nSliceNumber"></param>
        /// <param name="uiResponseAction"></param>
        public async Task<Action> UpdateRenderedObject(Orientation orientation, int nSliceNumber)
        {
            var mpr = this.MprImageModel;
            var mprOrientation = Mapper.Map<Orientation, MprImageModel.Orientation>(orientation);
            var bUpdated = MprImageModel.CheckAndUpdate(ref mpr, mprOrientation, nSliceNumber);
            if (bUpdated)
            {
                _counter.StartEvent();
                var pixels = await mpr.GetPixelsAsync();
                _counter.EndEvent();

                // perform the update on the UI thread
                return (() => UpdateImageSource(mpr, pixels));
            }

            return (() => { });
        }

        // 
        private void UpdateImageSource(MprImageModel mpr, byte[,] pixels)
        {
            int height = pixels.GetLength(0);
            int width = pixels.GetLength(1);

            byte[] flatPixels = FlattenPixels(pixels);            
            MprBuffer = BitmapSource.Create(width, height, 
                96.0, 96.0, PixelFormats.Gray8, null, flatPixels, width);

            // update the position
            ImagePosition = new TranslateTransform(-width / 2, -height / 2);
        }

        private static byte[] FlattenPixels(byte[,] pixels)
        {
            int height = pixels.GetLength(0);
            int width = pixels.GetLength(1);

            // flatten the pixels
            byte[] flatPixels = new byte[height * width];
            int n = 0;
            for (int r = 0; r < pixels.GetLength(0); r++)
                for (int c = 0; c < pixels.GetLength(1); c++, n++)
                    flatPixels[n] = pixels[r, c];
            return flatPixels;
        }
    }
}
