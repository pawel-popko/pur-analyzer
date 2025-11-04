namespace PurAnalyzer.Api.DTOs.Analyze;

/// <summary>
/// Business document header (H-row) with its line items (B-rows).
/// Represents one parsed document within a .PUR file.
/// </summary>
public sealed class DocumentDto
{
    /// <summary>
    /// Internal BA code.
    /// </summary>
    public string? BaCode { get; set; }

    /// <summary>
    /// Document type.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Document number.
    /// </summary>
    public string DocumentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Operation date of the document.
    /// </summary>
    public DateTime? OperationDate { get; set; }

    /// <summary>
    /// Sequential document day number.
    /// </summary>
    public int? DocumentDayNumber { get; set; }

    /// <summary>
    /// Contractor code.
    /// </summary>
    public string ContractorCode { get; set; } = string.Empty;

    /// <summary>
    /// Contractor name.
    /// </summary>
    public string? ContractorName { get; set; }

    /// <summary>
    /// External document number (reference).
    /// </summary>
    public string? ExternalDocumentNumber { get; set; }

    /// <summary>
    /// External document date.
    /// </summary>
    public DateTime? ExternalDocumentDate { get; set; }

    /// <summary>
    /// Net total amount for the document.
    /// </summary>
    public decimal? NetTotal { get; set; }

    /// <summary>
    /// VAT total amount for the document.
    /// </summary>
    public decimal? VatTotal { get; set; }

    /// <summary>
    /// Gross total amount for the document.
    /// </summary>
    public decimal? GrossTotal { get; set; }

    /// <summary>
    /// Custom flag 1.
    /// </summary>
    public string? Flag1 { get; set; }

    /// <summary>
    /// Custom flag 2.
    /// </summary>
    public string? Flag2 { get; set; }

    /// <summary>
    /// Custom flag 3.
    /// </summary>
    public string? Flag3 { get; set; }

    /// <summary>
    /// Document-level comment (row "C").
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Line items (B-rows) belonging to this document.
    /// </summary>
    public IEnumerable<DocumentItemDto> Items { get; set; } = Array.Empty<DocumentItemDto>();
}
