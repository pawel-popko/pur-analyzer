using PurAnalyzer.Application.Parsing;

namespace PurAnalyzer.Application.Analysis;

/// <summary>
/// Represents the computed results of .PUR file analysis.
/// </summary>
public sealed class AnalysisResult
{
    public IEnumerable<Document> Documents { get; init; } = Array.Empty<Document>();
    public int LineCount { get; init; }
    public int CharCount { get; init; }
    public int PositionsCount { get; init; }
    public int XCount { get; init; }

    /// <summary>
    /// Comma-separated names of products that share the highest total net value.
    /// </summary>
    public string ProductsWithMaxNetValue { get; init; } = string.Empty;

    /// <summary>
    /// Optional, user-friendly message summarizing non-standard scenarios.
    /// </summary>
    public string? Message { get; init; }
}