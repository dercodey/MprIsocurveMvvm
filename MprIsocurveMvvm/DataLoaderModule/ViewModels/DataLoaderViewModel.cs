using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Unity;

using Prism.Mvvm;
using Prism.Events;

using Infrastructure.Interfaces;
using Infrastructure.Events;

using DataLoaderModule.Interfaces;
using DataLoaderModule.Utilities;

namespace DataLoaderModule.ViewModels
{
    /// <summary>
    /// DataLoader is responsible for "loading" test data to display, which is usually just generated
    /// </summary>
    public class DataLoaderViewModel : BindableBase
    {
        IUnityContainer _container;
        IEventAggregator _eventAggregator;
        IModelRepository _repository;

        /// <summary>
        /// constructor via container will pass the event aggregator
        /// </summary>
        /// <param name="eventAggregator"></param>
        public DataLoaderViewModel(IUnityContainer container, IEventAggregator eventAggregator, IModelRepository repository)
        {
            _container = container;
            _eventAggregator = eventAggregator;
            _repository = repository;

            _dicomDirectory = @"C:\data\TestFullCTSlices";

            LoadImageVolumeCommand =
                new RelayCommand(() => LoadImageVolume(), () => true);
        }

        #region Load Image Volume

        /// <summary>
        /// 
        /// </summary>
        public string DicomDirectory
        {
            get { return _dicomDirectory; }
            set { SetProperty(ref _dicomDirectory, value); }
        }
        private string _dicomDirectory;

        /// <summary>
        /// performs the loading on a background thread, with status updates to the GUI
        /// </summary>
        void LoadImageVolume()
        {
            var context = SynchronizationContext.Current;

            // initialize load progress and start new task
            LoadProgress = 0;
            Task.Factory.StartNew(() => LoadImageVolumeAndReportStatus(context));
        }

        // background worker to actually perform the load and report status
        void LoadImageVolumeAndReportStatus(SynchronizationContext context)
        {            
            // construct the test data
            var loader = _container.Resolve<IDicomImageVolumeLoadService>();

            Guid volumeGuid = Guid.Empty;
            Func<int, int, int, IUniformImageVolumeModel> allocator =
                (width, height, depth) =>
                {
                    volumeGuid = _repository.CreateUniformImageVolume(width, height, depth);
                    return _repository.GetUniformImageVolume(volumeGuid);
                };
            
            bool bShowData = false;
            var statusStream = loader.LoadUniformImageVolumeFromDicom(DicomDirectory, allocator);
            int nUpdateCount = 0;
            foreach (var status in statusStream)
            {
                context.Send(ignore => LoadProgress = status.Progress, null);
                if (!bShowData && volumeGuid.CompareTo(Guid.Empty) != 0)
                { 
                    // now publish the load event
                    var showDataEvent = _eventAggregator.GetEvent<ImageDataLoadedEvent>();
                    showDataEvent.Publish(new ImageDataLoadedEventArgs()
                    {
                        ImageVolumeGuid = volumeGuid,
                    });

                    // don't show data again
                    bShowData = true;
                }

                if (status.ImageVolume != null)
                {
                    nUpdateCount++;
                    if (nUpdateCount % 30 == 0)
                    { 
                        // now publish the load event
                        var volumeUpdateEvent = _eventAggregator.GetEvent<VolumeUpdatedEvent>();
                        volumeUpdateEvent.Publish(new VolumeUpdatedEventArgs()
                        {
                            TimeStamp = DateTime.Now.Ticks,
                            ImageVolumeGuid = volumeGuid,
                        });
                    }
                }
            }

            //// now publish the load event
            //var showDataEvent = _eventAggregator.GetEvent<ShowDataEvent>();
            //showDataEvent.Publish(new ShowDataEventArgs()
            //{
            //    ImageVolumeGuid = volumeGuid,
            //});
        }

        /// <summary>
        /// represents the ICommand to start image volume loading
        /// </summary>
        public ICommand LoadImageVolumeCommand
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
