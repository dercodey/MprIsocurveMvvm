using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MprIsocurveGeneration
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return new Shell();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);

            var dataLoaderModuleInfo =
                new ModuleInfo("DataLoaderModule.Module",
                    typeof(DataLoaderModule.Module).AssemblyQualifiedName);
            moduleCatalog.AddModule(dataLoaderModuleInfo);

            var renderModuleInfo =
                new ModuleInfo("RenderModule.Module",
                    typeof(RenderModule.Module).AssemblyQualifiedName);
            moduleCatalog.AddModule(renderModuleInfo);

            var mprIsocurveGenerationModuleInfo =
                new ModuleInfo("MprIsocurveGeneration.Module",
                    typeof(MprIsocurveGeneration.Module).AssemblyQualifiedName);
            moduleCatalog.AddModule(mprIsocurveGenerationModuleInfo);
        }
    }
}
