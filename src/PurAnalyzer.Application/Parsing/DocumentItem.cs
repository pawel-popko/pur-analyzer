namespace PurAnalyzer.Application.Parsing;

/// <summary>
/// Parsed line item (B-row) belonging to a document.
/// Represents a single product record within a .PUR file.
/// </summary>
public sealed class DocumentItem
{
    /// <summary>
    /// Product identifier or SKU.
    /// </summary>
    public string? ProductCode { get; init; }

    /// <summary>
    /// Product name.
    /// </summary>
    public string? ProductName { get; init; }

    /// <summary>
    /// Quantity in the current document line.
    /// </summary>
    public decimal? Quantity { get; init; }

    /// <summary>
    /// Unit price (net).
    /// </summary>
    public decimal? UnitPriceNet { get; init; }

    /// <summary>
    /// Total net value for this line.
    /// </summary>
    public decimal? NetValue { get; init; }

    /// <summary>
    /// Total VAT value for this line.
    /// </summary>
    public decimal? VatValue { get; init; }

    /// <summary>
    /// Quantity before the operation (inventory state).
    /// </summary>
    public decimal? QuantityBefore { get; init; }

    /// <summary>
    /// Average unit price before the operation.
    /// </summary>
    public decimal? AverageBefore { get; init; }

    /// <summary>
    /// Quantity after the operation (inventory state).
    /// </summary>
    public decimal? QuantityAfter { get; init; }

    /// <summary>
    /// Average unit price after the operation.
    /// </summary>
    public decimal? AverageAfter { get; init; }

    /// <summary>
    /// Product group or category.
    /// </summary>
    public string? ProductGroup { get; init; }
}
