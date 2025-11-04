using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurAnalyzer.Domain.Parsing;

namespace PurAnalyzer.Infrastructure.Persistence.Config;

/// <summary>
/// EF Core mapping for DocumentItem.
/// </summary>
public sealed class DocumentItemConfig : IEntityTypeConfiguration<DocumentItem>
{
    public void Configure(EntityTypeBuilder<DocumentItem> b)
    {
        b.ToTable("document_items");
        b.HasKey(x => x.Id);

        // Text lengths
        b.Property(x => x.ProductCode).HasMaxLength(64);
        b.Property(x => x.ProductName).HasMaxLength(256);
        b.Property(x => x.ProductGroup).HasMaxLength(128);

        // Quantities and amounts – use numeric for precision
        b.Property(x => x.Quantity).HasColumnType("numeric(18,4)");
        b.Property(x => x.UnitPriceNet).HasColumnType("numeric(18,4)");
        b.Property(x => x.NetValue).HasColumnType("numeric(18,4)");
        b.Property(x => x.VatValue).HasColumnType("numeric(18,4)");
        b.Property(x => x.QuantityBefore).HasColumnType("numeric(18,4)");
        b.Property(x => x.AverageBefore).HasColumnType("numeric(18,4)");
        b.Property(x => x.QuantityAfter).HasColumnType("numeric(18,4)");
        b.Property(x => x.AverageAfter).HasColumnType("numeric(18,4)");

        // Required FK will be enforced by .HasForeignKey in DocumentConfig
        b.HasIndex(x => x.DocumentId);
    }
}
