using System.IO;
using System.Windows;
using APST.Entities;
using APST.Events;
using Syncfusion.Windows.PdfViewer;

namespace APST.ViewModels;

public class PdfViewModel : BindableBase
{
    private PdfViewerControl? _pdfViewerControl;

    public PdfViewModel(IEventAggregator eventAggregator)
    {
        eventAggregator.GetEvent<LoadPdfEvent>().Subscribe(OnNavigateCommand);
    }

    public DelegateCommand<RoutedEventArgs> BrowserLoadedCommand => new(OnBrowserLoaded);

    private void OnBrowserLoaded(RoutedEventArgs args)
    {
        if (args.Source is PdfViewerControl pdfViewerControl)
        {
            _pdfViewerControl = pdfViewerControl;
        }
    }

    private void OnNavigateCommand(SearchResult searchResult)
    {
        try
        {
            if (!File.Exists(searchResult.File))
            {
                MessageBox.Show($"PDF file not found: {searchResult.File}");
                return;
            }

            if (_pdfViewerControl == null)
            {
                return;
            }

            _pdfViewerControl.Load(searchResult.File);
            _pdfViewerControl.GotoPage(searchResult.Page);
        }
        catch (Exception e)
        {
            MessageBox.Show($"Error loading PDF: {e.Message}");
        }
    }
}