using AutoMapper;
using Unity;
using Unity.Lifetime;

using Prism.Events;
using Prism.Modularity;
using Prism.Regions;

using RenderModule.Interfaces;
using FsRenderModule.Interfaces;
using RenderModule.Models;
using RenderModule.Services;
using FsRenderModule.Services;
using RenderModule.ViewModels;
using RenderModule.Views;
using Prism.Ioc;


namespace RenderModule
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
            _regionManager.RegisterViewWithRegion("BlockLayoutRegion", typeof(BlockLayoutView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            _container.RegisterType<IMprGenerationFunction, MprGenerationFunction>(
                new TransientLifetimeManager());
            _container.RegisterType<IIsocurveFunction, IsocurveFunction>(
                new TransientLifetimeManager());
            _container.RegisterType<IFrameUpdateManager, FrameUpdateManager>(
                new ContainerControlledLifetimeManager());

            Mapper.Initialize(cfg => {
                cfg.CreateMap<PresentationStateViewModel.Orientation, Orientation>();
            });
        }
    }
}
