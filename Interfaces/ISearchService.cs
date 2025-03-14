namespace APST.Interfaces;

public interface ISearchService
{
    Task<List<int>> SearchTextInPdfAsync(string pdfPath, string searchText, CancellationToken cancellationToken);
}
