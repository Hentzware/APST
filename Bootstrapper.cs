using System.Windows;
using APST.Interfaces;
using APST.Services;
using APST.Views;

namespace APST;

public class Bootstrapper : PrismBootstrapper
{
    protected override DependencyObject CreateShell()
    {
        return Container.Resolve<MainView>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<ISearchService, SearchService>();
    }
}