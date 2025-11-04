using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurAnalyzer.Domain.Parsing;

namespace PurAnalyzer.Infrastructure.Persistence.Config;

/// <summary>
/// EF Core mapping for Document.
/// </summary>
public sealed class DocumentConfig : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> b)
    {
        b.ToTable("documents");
        b.HasKey(x => x.Id);

        // Text lengths – keep them sane to avoid unlimited varchars
        b.Property(x => x.BaCode).HasMaxLength(16);
        b.Property(x => x.Type).HasMaxLength(8);
        b.Property(x => x.DocumentNumber).HasMaxLength(64);
        b.Property(x => x.ContractorCode).HasMaxLength(64);
        b.Property(x => x.ContractorName).HasMaxLength(256);
        b.Property(x => x.ExternalDocumentNumber).HasMaxLength(128);
        b.Property(x => x.Flag1).HasMaxLength(32);
        b.Property(x => x.Flag2).HasMaxLength(32);
        b.Property(x => x.Flag3).HasMaxLength(32);
        b.Property(x => x.Comment).HasMaxLength(1024);

        // Monetary totals – map to numeric for precision
        b.Property(x => x.NetTotal).HasColumnType("numeric(18,4)");
        b.Property(x => x.VatTotal).HasColumnType("numeric(18,4)");
        b.Property(x => x.GrossTotal).HasColumnType("numeric(18,4)");

        // Dates without timezone — store as pure 'date'
        b.Property(x => x.OperationDate).HasColumnType("date");
        b.Property(x => x.ExternalDocumentDate).HasColumnType("date");

        // Relationship – cascade delete items when document is removed
        b.HasMany(x => x.Items)
         .WithOne(i => i.Document)
         .HasForeignKey(i => i.DocumentId)
         .OnDelete(DeleteBehavior.Cascade);

        // Optional: helpful non-unique index for typical lookups
        b.HasIndex(x => new { x.BaCode, x.Type, x.DocumentNumber, x.OperationDate });
    }
}
