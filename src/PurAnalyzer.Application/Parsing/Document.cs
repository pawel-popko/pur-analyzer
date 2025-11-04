namespace PurAnalyzer.Application.Parsing;

/// <summary>
/// Parsed business document header (H-row) with its line items (B-rows).
/// Neutral application model (no ASP.NET or EF dependencies).
/// </summary>
public sealed class Document
{
    // Header (H)
    public string? BaCode { get; init; }
    public string? Type { get; init; }

    public string? DocumentNumber { get; init; }
    public DateTime? OperationDate { get; init; }
    public int? DocumentDayNumber { get; init; }

    public string? ContractorCode { get; init; }
    public string? ContractorName { get; init; }

    public string? ExternalDocumentNumber { get; init; }
    public DateTime? ExternalDocumentDate { get; init; }

    public decimal? NetTotal { get; init; }
    public decimal? VatTotal { get; init; }
    public decimal? GrossTotal { get; init; }

    public string? Flag1 { get; init; }
    public string? Flag2 { get; init; }
    public string? Flag3 { get; init; }

    /// <summary>
    /// Comments from "C" rows associated with this document (order preserved from file).
    /// </summary>
    public string? Comment { get; init; }

    // Body (B)
    public IReadOnlyList<DocumentItem>? Items { get; init; } = Array.Empty<DocumentItem>();
}
