
using Microsoft.Practices.Unity;

using Prism.Modularity;
using Prism.Regions;
using Prism.Events;

using RenderModule.Interfaces;
using RenderModule.Services;

using MprIsocurveGeneration.Views;
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
            _regionManager.RegisterViewWithRegion("SidebarRegion", typeof(IsocurveControlView));
        }
    }
}
