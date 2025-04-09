using System.IO;
using System.Windows;
using Syncfusion.Licensing;
using Path = System.IO.Path;

namespace APST;

public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        var licenseFile = Path.Combine(Directory.GetCurrentDirectory(), "apst.lic");

        if (File.Exists(licenseFile))
        {
            var key = File.ReadAllText(licenseFile);
            SyncfusionLicenseProvider.RegisterLicense(key);
        }
        else
        {
            MessageBox.Show("License not found. Closing app.");
            Shutdown();
        }

        var bootstrapper = new Bootstrapper();
        bootstrapper.Run();
    }
}