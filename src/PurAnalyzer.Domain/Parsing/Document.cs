namespace PurAnalyzer.Domain.Parsing;

/// <summary>
/// Business document parsed from .PUR file; persisted only when validation succeeds.
/// </summary>
public sealed class Document
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // --- Business data parsed from the file ---
    public string? BaCode { get; set; }
    public string? Type { get; set; }
    public string? DocumentNumber { get; set; }
    public DateTime? OperationDate { get; set; }
    public int? DocumentDayNumber { get; set; }
    public string? ContractorCode { get; set; }
    public string? ContractorName { get; set; }
    public string? ExternalDocumentNumber { get; set; }
    public DateTime? ExternalDocumentDate { get; set; }
    public decimal? NetTotal { get; set; }
    public decimal? VatTotal { get; set; }
    public decimal? GrossTotal { get; set; }
    public string? Flag1 { get; set; }
    public string? Flag2 { get; set; }
    public string? Flag3 { get; set; }
    public string? Comment { get; set; }

    // --- Navigation ---
    public ICollection<DocumentItem> Items { get; set; } = new List<DocumentItem>();
}
