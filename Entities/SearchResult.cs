namespace APST.Entities;

public class SearchResult
{
    public int Page { get; set; }

    public string File { get; set; } = string.Empty;

    public string SearchTerm { get; set; } = string.Empty;

    public bool NoResults { get; set; }
}