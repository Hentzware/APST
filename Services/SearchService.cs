using APST.Interfaces;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace APST.Services;

public class SearchService : ISearchService
{
    public async Task<List<int>> SearchTextInPdfAsync(string pdfPath, string searchText, CancellationToken cancellationToken)
    {
        return await Task.Run(async () =>
        {
            var results = new List<int>();

            using var pdfReader = new PdfReader(pdfPath);
            using var pdfDocument = new PdfDocument(pdfReader);

            for (var page = 1; page <= pdfDocument.GetNumberOfPages(); page++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                var currentPageText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(page), strategy);

                if (currentPageText.ToLower().Contains(searchText))
                {
                    results.Add(page);
                }

                await Task.Yield(); // Yield to keep the UI responsive
            }

            return results;
        }, cancellationToken);
    }
}