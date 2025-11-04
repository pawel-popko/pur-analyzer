namespace PurAnalyzer.Api.DTOs.Analyze;

/// <summary>
/// Single line item (B-row) within a .PUR document.
/// Numeric fields are tolerant to missing or invalid values.
/// </summary>
public sealed class DocumentItemDto
{
    /// <summary>
    /// Product identifier or SKU.
    /// </summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>
    /// Full product name.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Quantity in the current document line.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Unit price (net).
    /// </summary>
    public decimal UnitPriceNet { get; set; }

    /// <summary>
    /// Total net value (quantity × unit price).
    /// </summary>
    public decimal NetValue { get; set; }

    /// <summary>
    /// VAT amount for this line.
    /// </summary>
    public decimal VatValue { get; set; }

    /// <summary>
    /// Quantity before the operation (inventory state).
    /// </summary>
    public decimal QuantityBefore { get; set; }

    /// <summary>
    /// Average unit price before the operation.
    /// </summary>
    public decimal AverageBefore { get; set; }

    /// <summary>
    /// Quantity after the operation (inventory state).
    /// </summary>
    public decimal QuantityAfter { get; set; }

    /// <summary>
    /// Average unit price after the operation.
    /// </summary>
    public decimal AverageAfter { get; set; }

    /// <summary>
    /// Product group or category.
    /// </summary>
    public string? ProductGroup { get; set; }
}
