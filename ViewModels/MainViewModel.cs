using APST.Constants;
using APST.Views;

namespace APST.ViewModels;

public class MainViewModel : BindableBase
{
    public MainViewModel(RegionManager regionManager)
    {
        regionManager.RegisterViewWithRegion<SearchView>(Regions.SearchRegion);
        regionManager.RegisterViewWithRegion<PdfView>(Regions.PdfRegion);
    }
}