using System.Windows;

namespace APST;

public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        var bootstrapper = new Bootstrapper();
        bootstrapper.Run();
    }
}