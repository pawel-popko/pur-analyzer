using System;

namespace PurAnalyzer.Domain.Parsing;

/// <summary>
/// Single line item belonging to a business document parsed from .PUR file.
/// </summary>
public sealed class DocumentItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // --- Item data parsed from the file ---
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPriceNet { get; set; }
    public decimal? NetValue { get; set; }
    public decimal? VatValue { get; set; }
    public decimal? QuantityBefore { get; set; }
    public decimal? AverageBefore { get; set; }
    public decimal? QuantityAfter { get; set; }
    public decimal? AverageAfter { get; set; }
    public string? ProductGroup { get; set; }

    // --- Relationships ---
    // Foreign key to parent Document; required at persistence layer.
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
}
