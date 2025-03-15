using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using APST.Entities;
using APST.Events;
using APST.Interfaces;
using DialogResult = System.Windows.Forms.DialogResult;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace APST.ViewModels;

public class SearchViewModel : BindableBase
{
    private const string CacheFilePath = "searchResultsCache.json";
    private readonly IEventAggregator _eventAggregator;
    private readonly ISearchService _searchService;
    private bool _isConfigEnabled = true;
    private CancellationTokenSource? _cts;
    private ObservableCollection<SearchResult> _searchResults = new();
    private SearchResult? _selectedSearchResult;
    private string _pdfFolderPath = string.Empty;
    private string _searchText = string.Empty;
    private Visibility _searchingVisibility = Visibility.Collapsed;

    public SearchViewModel(ISearchService searchService, IEventAggregator eventAggregator)
    {
        _searchService = searchService;
        _eventAggregator = eventAggregator;
        PdfFolderPath = Settings.Default.SearchPath;
        LoadSearchResultsFromCache();
    }

    public AsyncDelegateCommand CancelCommand => new(OnCancelAsync);

    public AsyncDelegateCommand SearchCommand => new(OnSearchAsync);

    public AsyncDelegateCommand<KeyEventArgs> KeyDownCommand => new(OnKeyDownAsync);

    public bool IsConfigEnabled
    {
        get => _isConfigEnabled;
        set => SetProperty(ref _isConfigEnabled, value);
    }

    public DelegateCommand OpenFolderPathCommand => new(OnOpenFolderPath);

    public ObservableCollection<SearchResult> SearchResults
    {
        get => _searchResults;
        set => SetProperty(ref _searchResults, value);
    }

    public SearchResult? SelectedSearchResult
    {
        get => _selectedSearchResult;
        set
        {
            SetProperty(ref _selectedSearchResult, value);
            if (value != null)
            {
                _eventAggregator.GetEvent<LoadPdfEvent>().Publish(value);
            }
        }
    }

    public string PdfFolderPath
    {
        get => _pdfFolderPath;
        set
        {
            SetProperty(ref _pdfFolderPath, value);
            Settings.Default.SearchPath = value;
            Settings.Default.Save();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public Visibility SearchingVisibility
    {
        get => _searchingVisibility;
        set => SetProperty(ref _searchingVisibility, value);
    }

    private ObservableCollection<SearchResult> LoadSearchResultsFromCache()
    {
        if (!File.Exists(CacheFilePath))
        {
            return new ObservableCollection<SearchResult>();
        }

        var json = File.ReadAllText(CacheFilePath);
        var searchResults = JsonSerializer.Deserialize<ObservableCollection<SearchResult>>(json);

        if (searchResults == null)
        {
            return new ObservableCollection<SearchResult>();
        }

        var validResults = searchResults.Where(r => r.NoResults || File.Exists(r.File)).ToList();

        SaveSearchResultsToCache(validResults);

        return new ObservableCollection<SearchResult>(validResults);
    }

    private async Task OnCancelAsync()
    {
        if (_cts == null)
        {
            return;
        }

        await _cts.CancelAsync();
    }

    private async Task OnKeyDownAsync(KeyEventArgs arg)
    {
        if (arg.Key == Key.Enter && arg.IsDown && IsConfigEnabled)
        {
            await OnSearchAsync();
        }
    }

    private async Task OnSearchAsync()
    {
        SearchingVisibility = Visibility.Visible;
        IsConfigEnabled = false;
        _cts = new CancellationTokenSource();
        var cancellationToken = _cts.Token;
        var existingResults = LoadSearchResultsFromCache();
        var searchTextLower = SearchText.ToLower();
        var cachedResults = existingResults.Where(r => r.SearchTerm.ToLower() == searchTextLower).ToList();
        var files = Directory.GetFiles(PdfFolderPath, "*.pdf");
        var cachedFiles = cachedResults.Select(r => r.File).Distinct().ToList();
        var newFiles = files.Except(cachedFiles).ToList();

        try
        {
            if (!newFiles.Any())
            {
                SearchResults = new ObservableCollection<SearchResult>(cachedResults.Where(r => !r.NoResults));
                return;
            }

            var searchResults = new List<SearchResult>();
            var searchTasks = newFiles.Select(file => _searchService.SearchTextInPdfAsync(file, searchTextLower, cancellationToken)).ToList();

            var results = await Task.WhenAll(searchTasks);

            for (var i = 0; i < newFiles.Count; i++)
            {
                if (results[i].Any())
                {
                    searchResults.AddRange(results[i].Select(page => new SearchResult { File = newFiles[i], Page = page, SearchTerm = searchTextLower }));
                }
                else
                {
                    searchResults.Add(new SearchResult { File = newFiles[i], SearchTerm = searchTextLower, NoResults = true });
                }
            }

            foreach (var result in searchResults)
            {
                if (!existingResults.Any(r => r.File == result.File && r.Page == result.Page && r.SearchTerm.ToLower() == result.SearchTerm.ToLower()))
                {
                    existingResults.Add(result);
                }
            }

            SearchResults = new ObservableCollection<SearchResult>(existingResults.Where(r => r.SearchTerm.ToLower() == searchTextLower && !r.NoResults));
            SaveSearchResultsToCache(existingResults);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            SearchingVisibility = Visibility.Collapsed;
            IsConfigEnabled = true;
        }
    }

    private void OnOpenFolderPath()
    {
        using var dialog = new FolderBrowserDialog();
        dialog.ShowNewFolderButton = false;

        var result = dialog.ShowDialog();

        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
        {
            PdfFolderPath = dialog.SelectedPath;
        }
    }

    private void SaveSearchResultsToCache(IEnumerable<SearchResult> searchResults)
    {
        var json = JsonSerializer.Serialize(searchResults);
        File.WriteAllText(CacheFilePath, json);
    }
}