using System.Windows;

using Prism.Unity;
using Prism.Modularity;

namespace MprIsocurveGeneration
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return new Shell();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            App.Current.MainWindow = (Window)this.Shell;
            App.Current.MainWindow.Show();
        }

        protected override void ConfigureModuleCatalog()
        {
            base.ConfigureModuleCatalog();

            var dataLoaderModuleInfo = 
                new ModuleInfo("DataLoaderModule.Module", 
                    typeof(DataLoaderModule.Module).AssemblyQualifiedName);
            ModuleCatalog.AddModule(dataLoaderModuleInfo);

            var renderModuleInfo =
                new ModuleInfo("RenderModule.Module",
                    typeof(RenderModule.Module).AssemblyQualifiedName);
            ModuleCatalog.AddModule(renderModuleInfo);

            var mprIsocurveGenerationModuleInfo = 
                new ModuleInfo("MprIsocurveGeneration.Module", 
                    typeof(MprIsocurveGeneration.Module).AssemblyQualifiedName);
            ModuleCatalog.AddModule(mprIsocurveGenerationModuleInfo);
        }
    }
}
