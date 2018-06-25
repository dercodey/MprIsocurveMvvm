
using Microsoft.Practices.Unity;

using Prism.Modularity;
using Prism.Regions;
using Prism.Events;

using MprIsocurveGeneration.Views;

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

        public void Initialize()
        {
            _regionManager.RegisterViewWithRegion("SidebarRegion", typeof(IsocurveControlView));
        }
    }
}
