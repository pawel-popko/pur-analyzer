namespace PurAnalyzer.Api.DTOs.Analyze;

/// <summary>
/// Stable v1 response contract for the .PUR analysis endpoint.
/// </summary>
public sealed class AnalyzeResponseDto
{
    /// <summary>Parsed documents returned by the analyzer (empty if none).</summary>
    public IEnumerable<DocumentDto> Documents { get; set; } = Array.Empty<DocumentDto>();

    /// <summary>Total number of lines in the uploaded file.</summary>
    public int LineCount { get; set; }

    /// <summary>Total number of characters in the uploaded file.</summary>
    public int CharCount { get; set; }

    /// <summary>Total number of item positions across all documents.</summary>
    public int PositionsCount { get; set; }

    /// <summary>Number of documents that contain more item positions than the input parameter <c>x</c>.</summary>
    public int XCount { get; set; }

    /// <summary>
    /// Comma-separated list of product names with the highest total net value
    /// (aggregation by productCode; when multiple codes share the same max sum).
    /// </summary>
    public string ProductsWithMaxNetValue { get; set; } = string.Empty;

    /// <summary>Optional, user-friendly message for non-standard but successful scenarios.</summary>
    public string? Message { get; set; }
}
