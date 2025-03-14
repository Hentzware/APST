using System.Windows;
using System.Windows.Controls;
using APST.Events;

namespace APST.ViewModels;

public class PdfViewModel : BindableBase
{
    private WebBrowser? _webBrowser;

    public PdfViewModel(IEventAggregator eventAggregator)
    {
        eventAggregator.GetEvent<LoadPdfEvent>().Subscribe(OnNavigateCommand);
    }

    public DelegateCommand<RoutedEventArgs> BrowserLoadedCommand => new(OnBrowserLoaded);

    private void OnBrowserLoaded(RoutedEventArgs args)
    {
        if (args.Source is WebBrowser webBrowser)
        {
            _webBrowser = webBrowser;
        }
    }

    private async void OnNavigateCommand(SearchResult searchResult)
    {
        if (_webBrowser != null)
        {
            var fileUri = new Uri(searchResult.File, UriKind.Absolute);
            var pageUri = new Uri($"{fileUri.AbsoluteUri}#page={searchResult.Page}");

            // Navigate to an empty page to force navigation
            _webBrowser.Navigate("about:blank");

            // Delay to ensure the WebBrowser has time to navigate to the blank page
            await Task.Delay(100);

            _webBrowser.Navigate(pageUri);
        }
    }
}