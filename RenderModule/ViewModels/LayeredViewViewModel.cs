using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

using Microsoft.Practices.Unity;

using Prism.Events;
using Prism.Mvvm;

using AutoMapper;

using Infrastructure.Utilities;
using Infrastructure.Events;
using Infrastructure.Interfaces;

using RenderModule.Interfaces;
using RenderModule.Models;
using RenderModule.ViewModels;

namespace RenderModule.ViewModels
{
    /// <summary>
    /// main LayeredViewViewModel is created by auto-wiring from the view,
    ///     so anything it needs access to is passed in from the Unity container
    /// </summary>
    public class LayeredViewViewModel : BindableBase
    {
        IUnityContainer _container;
        IEventAggregator _eventAggregator;
        IModelRepository _repository;

        SynchronizationContext _uiUpdateContext;
        IFrameUpdateManager _renderUpdateManager;

        /// <summary>
        /// constructs a new LayeredViewViewModel, and wires up the necessary events
        /// </summary>
        /// <param name="container">the unity container for resolving things</param>
        /// <param name="eventAggregator">the event aggregator for sending/receiving</param>
        /// <param name="repository">the model repository</param>
        public LayeredViewViewModel(IUnityContainer container, 
            IEventAggregator eventAggregator,
            IModelRepository repository)
        {
            _container = container;
            _eventAggregator = eventAggregator;
            _repository = repository;

            // store the UI thread SynchronizationContext
            _uiUpdateContext = SynchronizationContext.Current;
            System.Diagnostics.Trace.Assert(_uiUpdateContext != null);

            // create a new PresentationState view model and add to the active layer
            PresentationState = _container.Resolve<PresentationStateViewModel>();
            ActiveLayer.Add(PresentationState);

            // subscribe to events
            _eventAggregator.GetEvent<NavigationPointUpdateEvent>()
                .Subscribe(OnNavigationPointUpdated, ThreadOption.UIThread);
            _eventAggregator.GetEvent<ImageDataLoadedEvent>()
                .Subscribe(OnImageDataLoadedEvent, ThreadOption.UIThread);
            _eventAggregator.GetEvent<VolumeUpdatedEvent>()
                .Subscribe(OnVolumeUpdatedEvent, ThreadOption.UIThread);
            _eventAggregator.GetEvent<SetIsocurveLevelEvent>()
                .Subscribe(OnSetIsocurveLevel, ThreadOption.UIThread);

            // set up the performance counters
            FrameRatePerformance = new PerformanceCounter("FrameRate", _uiUpdateContext);
            BackgroundLayerPerformance = new PerformanceCounter("BackLayer", _uiUpdateContext);
            PassiveLayerPerformance = new PerformanceCounter("PassLayer", _uiUpdateContext);

            _renderUpdateManager = _container.Resolve<IFrameUpdateManager>();
            if (_renderUpdateManager.UiUpdatePerformance == null)
            { 
                _renderUpdateManager.UiUpdatePerformance = new PerformanceCounter("UIUpdate", _uiUpdateContext);
            }
            UiUpdatePerformance = _renderUpdateManager.UiUpdatePerformance;
        }

        /// <summary>
        /// represents the view's presentation state
        /// </summary>
        public PresentationStateViewModel PresentationState { get; }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<BindableBase> ActiveLayer { get; } = new ObservableCollection<BindableBase>();

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<BindableBase> PassiveLayer { get; } = new ObservableCollection<BindableBase>();

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<BindableBase> BackgroundLayer { get; } = new ObservableCollection<BindableBase>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        void OnNavigationPointUpdated(NavigationPointUpdateEventArgs e)
        {
            // TODO: report the time stamp of this
            if (e.Sender != PresentationState)
            {
                _uiUpdateContext.Send(
                    point => PresentationState.SetNavigationPointNoEvent((Point3D)point),
                    e.NavigationPointPosition);
            }

            // TODO: report the time stamp of the update event
            UpdateAllRendered(e.TimeStamp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        void OnImageDataLoadedEvent(ImageDataLoadedEventArgs e)
        {
            var mprImageModel = CreateMprImage(e.ImageVolumeGuid);

            // remove any MPRs vms from background layer
            BackgroundLayer.OfType<MprImageViewModel>()
                .ToList().ForEach(vm => BackgroundLayer.Remove(vm));

            // create a new MPR vm
            var mprImageVm = _container.Resolve<MprImageViewModel>();
            mprImageVm.SetPerformanceCounter(_backgroundLayerPerformance);
            mprImageVm.MprImageModel = mprImageModel;
            BackgroundLayer.Add(mprImageVm);

            // setup the isocurves
            SetupIsocurves(mprImageModel, 10);

            // and perform the update
            UpdateAllRendered(DateTime.Now.Ticks);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputVolumeGuid"></param>
        /// <returns></returns>
        public MprImageModel CreateMprImage(Guid inputVolumeGuid)
        {
            var guid = Guid.NewGuid();
            var mprImageModel = _container.Resolve<MprImageModel>();
            mprImageModel.InputVolumeGuid = inputVolumeGuid;
            mprImageModel.InputVolume = _repository.GetUniformImageVolume(inputVolumeGuid);
            return mprImageModel;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        void OnVolumeUpdatedEvent(VolumeUpdatedEventArgs e)
        {
            UpdateAllRendered(e.TimeStamp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        void OnSetIsocurveLevel(SetIsocurveLevelEventArgs e)
        {
            // set the levels and setup
            SetupIsocurves(null, e.Levels);

            // and perform updates
            UpdateAllRendered(DateTime.Now.Ticks);
        }

        // creates and sets up the isocurve VM
        private void SetupIsocurves(MprImageModel mprImageModel, int nLevels)
        {
            var isocurveVm = PassiveLayer.OfType<IsocurveViewModel>().FirstOrDefault();
            if (isocurveVm == null)
            {
                isocurveVm = _container.Resolve<IsocurveViewModel>();
                PassiveLayer.Add(isocurveVm);
                isocurveVm.SetPerformanceCounter(_passiveLayerPerformance);
            }
            isocurveVm.PopulateIsocurveRange(mprImageModel, nLevels);
        }

        /// <summary>
        /// helper to update all IRenderedObjectViewModels in the layers
        /// </summary>
        /// <param name="responseContext"></param>
        private void UpdateAllRendered(Int64 timeStamp)
        {
            UpdateSingleLayer(timeStamp, BackgroundLayer);
            UpdateSingleLayer(timeStamp, PassiveLayer);
            UpdateSingleLayer(timeStamp, ActiveLayer);

            // call to process any completed update tassks
            // _renderUpdateManager.ProcessQueue();

            // ending and then starting again to calculate the frame rate
            FrameRatePerformance.EndEvent();
            FrameRatePerformance.StartEvent();
        }

        // helper to update a single layer
        private void UpdateSingleLayer(Int64 timeStamp, IEnumerable<BindableBase> layer)
        {
            foreach (var ro in layer.OfType<IRenderedObject>())
            {
                // get the task that is producing the update action
                Task<Action> uiUpdateAction = 
                    ro.UpdateRenderedObject(Mapper.Map<PresentationStateViewModel.Orientation,
                                                MprImageModel.Orientation>(PresentationState.ViewOrientation),
                        (int) PresentationState.SlicePosition);

                // queue up the task
                _renderUpdateManager.QueueAction((UInt64)timeStamp, 
                    _uiUpdateContext, uiUpdateAction);
            }
        }

        /// <summary>
        /// counter for frame rate
        /// </summary>
        public PerformanceCounter FrameRatePerformance
        {
            get { return _frameRatePerformance; }
            set { SetProperty(ref _frameRatePerformance, value); }
        }
        private PerformanceCounter _frameRatePerformance;

        /// <summary>
        /// counter for background layer update
        /// </summary>
        public PerformanceCounter BackgroundLayerPerformance
        {
            get { return _backgroundLayerPerformance; }
            set { SetProperty(ref _backgroundLayerPerformance, value); }
        }
        PerformanceCounter _backgroundLayerPerformance;

        /// <summary>
        /// counter for passive layer update
        /// </summary>
        public PerformanceCounter PassiveLayerPerformance
        {
            get { return _passiveLayerPerformance; }
            set { SetProperty(ref _passiveLayerPerformance, value); }
        }
        PerformanceCounter _passiveLayerPerformance;

        /// <summary>
        /// counter for UI update actions (on the UI thread)
        /// </summary>
        public PerformanceCounter UiUpdatePerformance
        {
            get { return _uiUpdateMS; }
            set { SetProperty(ref _uiUpdateMS, value); }
        }
        PerformanceCounter _uiUpdateMS;
    }
}
