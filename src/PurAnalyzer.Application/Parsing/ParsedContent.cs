namespace PurAnalyzer.Application.Parsing;

/// <summary>
/// Represents the structured result of a parsed .PUR file.
/// </summary>
public sealed class ParsedContent
{
    /// <summary>
    /// All raw lines read from the file.
    /// </summary>
    public IReadOnlyList<string> Lines { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Parsed business documents (H rows with their B rows).
    /// </summary>
    public IReadOnlyList<Document> Documents { get; init; } = Array.Empty<Document>();

    /// <summary>
    /// Total number of characters in the file.
    /// </summary>
    public int CharCount { get; init; }

    /// <summary>
    /// Total number of lines in the file.
    /// </summary>
    public int LineCount { get; init; }

    /// <summary>
    /// Total number of item positions across all documents.
    /// </summary>
    public int PositionsCount { get; init; }

    /// <summary>
    /// Indicates whether the file passed basic format validation.
    /// </summary>
    public bool IsFormatValid { get; init; } = true;

    /// <summary>
    /// List of detected format issues, if any.
    /// </summary>
    public IReadOnlyList<string> FormatIssues { get; init; } = Array.Empty<string>();
}
