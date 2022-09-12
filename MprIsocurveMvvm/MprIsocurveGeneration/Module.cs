
using Unity;

using Prism.Modularity;
using Prism.Regions;
using Prism.Events;

using MprIsocurveGeneration.Views;
using Prism.Ioc;

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
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RegisterViewWithRegion("SidebarRegion", typeof(IsocurveControlView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }
    }
}
