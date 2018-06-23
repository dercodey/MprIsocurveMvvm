using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Practices.Unity;

using Prism.Modularity;
using Prism.Regions;
using Prism.Events;

using MprIsocurveGeneration.Models;
using MprIsocurveGeneration.Views;
using MprIsocurveGeneration.ViewModels;
using MprIsocurveGeneration.Services;

namespace MprIsocurveGeneration
{
    public class Module : IModule
    {
        IUnityContainer _container;
        IRegionManager _regionManager;
        IEventAggregator _eventAggregator;

        public Module(IUnityContainer container, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _container = container;
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;

            // register the model repository and other state-holding services
            _container.RegisterType<IModelRepository, ModelRepository>(
                new ContainerControlledLifetimeManager());
            _container.RegisterType<ITransformationGraphRepository, TransformationGraphRepository>(
                new ContainerControlledLifetimeManager());

            // register other services that don't maintain state
            _container.RegisterType<IDicomImageVolumeLoadService, DicomImageVolumeLoadService>(
                new TransientLifetimeManager());
            _container.RegisterType<IGaussianVolumeFunction, GaussianVolumeFunction>(
                new TransientLifetimeManager());
            _container.RegisterType<IMprGenerationFunction, MprGenerationFunction>(
                new TransientLifetimeManager());
            _container.RegisterType<IIsocurveFunction, IsocurveFunction>(
                new TransientLifetimeManager());
            _container.RegisterType<IFrameUpdateManager, FrameUpdateManager>(
                new ContainerControlledLifetimeManager());
                // new TransientLifetimeManager());
        }

        public void Initialize()
        {
            _regionManager.RegisterViewWithRegion("BlockLayoutRegion", typeof(BlockLayoutView));
            _regionManager.RegisterViewWithRegion("SidebarRegion", typeof(DataLoaderView));
            _regionManager.RegisterViewWithRegion("SidebarRegion", typeof(DataGenerateView));
            _regionManager.RegisterViewWithRegion("SidebarRegion", typeof(IsocurveControlView));
        }
    }
}
