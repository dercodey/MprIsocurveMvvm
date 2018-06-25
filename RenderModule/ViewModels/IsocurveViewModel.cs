using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Practices.Unity;

using Prism.Mvvm;

using Infrastructure.Utilities;
using Infrastructure.Interfaces;

using RenderModule.Models;
using RenderModule.Interfaces;


namespace RenderModule.ViewModels
{
    using Orientation = MprImageModel.Orientation;

    /// <summary>
    /// Generates and update isocurves bindable to the Geometry path
    /// </summary>
    public class IsocurveViewModel : BindableBase, IRenderedObject
    {
        IUnityContainer _container;
        IModelRepository _repository;

        public IsocurveViewModel(IUnityContainer container, IModelRepository repository)
        {
            _container = container;
            _repository = repository;
        }

        /// <summary>
        /// generates a range of isocurves
        /// </summary>
        /// <param name="mprGuid">the MPR guid for the isocurves</param>
        /// <param name="levels">number of levels</param>
        /// <returns>collection of new isocurve VMs</returns>
        public void PopulateIsocurveRange(MprImageModel mprImageModel, int levels)
        {
            // create the new isocurve vms
            var isocurveLevels = 
                from level in Enumerable.Range(1, levels)
                select new IsocurveLevel()
                {
                    Threshold = (float)level * 20,
                    CurveColor = GetColorwashBrush((double)(level-1) / (double)levels)
                };

            if (mprImageModel != null)
                MprImageModel = mprImageModel;
            IsocurveLevels = new ObservableCollection<IsocurveLevel>(isocurveLevels.ToList());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hue">hue value from 0.0 to 1.0</param>
        /// <returns></returns>
        public static Brush GetColorwashBrush(double hue)
        {
            if (_rainbow == null)
            {
                var rainbowUri =
                    new Uri(@"pack://application:,,,/Resources/Rainbow.bmp", UriKind.RelativeOrAbsolute);
                _rainbow = new BitmapImage(rainbowUri);
            }

            int y = (int) (hue * (_rainbow.PixelHeight-1));
            CroppedBitmap cb = new CroppedBitmap(_rainbow, new Int32Rect(1, y, 1, 1));
            byte[] pixel = new byte[_rainbow.Format.BitsPerPixel / 8];
            cb.CopyPixels(pixel, _rainbow.Format.BitsPerPixel / 8, 0);
            return new SolidColorBrush(Color.FromRgb(pixel[2], pixel[1], pixel[0]));
        }

        static BitmapImage _rainbow;

        /// <summary>
        /// stores the GUID of the associated MPR model object
        /// </summary>
        public MprImageModel MprImageModel
        {
            get { return _mprImageModel; }
            set { SetProperty(ref _mprImageModel, value); }
        }
        MprImageModel _mprImageModel;

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<IsocurveLevel> IsocurveLevels
        {
            get { return _isocurveLevels; }
            set { SetProperty(ref _isocurveLevels, value); }
        }
        private ObservableCollection<IsocurveLevel> _isocurveLevels =
            new ObservableCollection<IsocurveLevel>();

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
        /// method to update the rendered object based on presentation state
        /// </summary>
        /// <param name="orientation"></param>
        /// <param name="nSliceNumber"></param>
        /// <param name="uiResponseAction"></param>
        public async Task<Action> UpdateRenderedObject(Orientation orientation, int nSliceNumber)
        {
            // get and attempt update of the MPR image
            var mpr = MprImageModel;

            var updateTask = 
                Task.Run<Action>(async () =>
                {
                    // contains the actions to update each isocurve
                    var isocurveUpdateActions = new List<Action>();

                    var isocurveFunction = _container.Resolve<IIsocurveFunction>();

                    _counter.StartEvent();

                    foreach (var isocurveLevel in IsocurveLevels)
                    {
                        var geometry = await 
                            isocurveFunction.GenerateIsocurveAsync(mpr, isocurveLevel.Threshold);

                        // add level with its geometry
                        isocurveUpdateActions.Add(() => isocurveLevel.UpdateGeometry(geometry));
                    }

                    _counter.EndEvent();

                    // on UI thread need to update the bound geometry
                    return (() => isocurveUpdateActions.ForEach(action => action.Invoke()));
                });

            return await updateTask;
        }

    }
}
