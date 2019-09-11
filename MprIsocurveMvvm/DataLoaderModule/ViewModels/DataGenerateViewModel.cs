using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Prism.Mvvm;
using Prism.Events;
using Microsoft.Practices.Unity;

using Infrastructure.Interfaces;
using Infrastructure.Events;

using DataLoaderModule.Interfaces;
using DataLoaderModule.Utilities;


namespace DataLoaderModule.ViewModels
{
    /// <summary>
    /// DataLoader is responsible for "loading" test data to display, which is usually just generated
    /// </summary>
    public class DataGenerateViewModel : BindableBase
    {
        IUnityContainer _container;
        IEventAggregator _eventAggregator;
        IModelRepository _repository;

        /// <summary>
        /// constructor via container will pass the event aggregator
        /// </summary>
        /// <param name="eventAggregator"></param>
        public DataGenerateViewModel(IUnityContainer container,
            IEventAggregator eventAggregator, IModelRepository repository)
        {
            _container = container;
            _eventAggregator = eventAggregator;
            _repository = repository;

            GenerateImageVolumeCommand =
                new RelayCommand(() => GenerateImageVolume(), () => true);
        }

        #region Generate Image Volume

        /// <summary>
        /// width of created volume
        /// </summary>
        public int Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }
        private int _width = 256;

        /// <summary>
        /// height of created volume
        /// </summary>
        public int Height
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }
        private int _height = 256;

        /// <summary>
        /// depth of created volume
        /// </summary>
        public int Depth
        {
            get { return _depth; }
            set { SetProperty(ref _depth, value); }
        }
        private int _depth = 128;

        /// <summary>
        /// performs the loading on a background thread, with status updates to the GUI
        /// </summary>
        void GenerateImageVolume()
        {
            var context = SynchronizationContext.Current;

            // initialize load progress and start new task
            LoadProgress = 0;
            Task.Factory.StartNew(() => GenerateImageVolumeAndReportStatus(context));
        }

        // background worker to actually perform the load and report status
        void GenerateImageVolumeAndReportStatus(SynchronizationContext context)
        {
            // construct the test data
            var guid = _repository.CreateUniformImageVolume(Width, Height, Depth);
            var gauss = _container.Resolve<IGaussianVolumeFunction>();
            var imageVolume = _repository.GetUniformImageVolume(guid);
            var statusStream = gauss.PopulateGaussian(imageVolume);
            foreach (var status in statusStream)
            {
                context.Post(ignore => LoadProgress = status.Progress, null);
            }

            // now publish the load event
            var loadEvent = _eventAggregator.GetEvent<ImageDataLoadedEvent>();
            loadEvent.Publish(new ImageDataLoadedEventArgs() 
            { 
                ImageVolumeGuid = guid,
            });
        }

        /// <summary>
        /// represents the ICommand to start image volume loading
        /// </summary>
        public ICommand GenerateImageVolumeCommand
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// binds to progress bar to indicate amount loaded
        /// </summary>
        public int LoadProgress
        {
            get { return _loadProgress; }
            set { SetProperty(ref _loadProgress, value); }
        }
        int _loadProgress = 0;
    }
}
