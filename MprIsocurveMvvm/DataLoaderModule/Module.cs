using Unity;
using Unity.Lifetime;

using Prism.Events;
using Prism.Modularity;
using Prism.Regions;

using Infrastructure.Interfaces;

using DataLoaderModule.Interfaces;
using DataLoaderModule.Services;
using DataLoaderModule.Views;
using Prism.Ioc;

namespace DataLoaderModule
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
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RegisterViewWithRegion("SidebarRegion", typeof(DataLoaderView));
            _regionManager.RegisterViewWithRegion("SidebarRegion", typeof(DataGenerateView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
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

        }
    }
}
