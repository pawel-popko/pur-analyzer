using PurAnalyzer.Application.Analysis;
using PurAnalyzer.Application.Parsing;
using Dom = PurAnalyzer.Domain.Parsing;

namespace PurAnalyzer.Infrastructure.Persistence.Services;

/// <summary>
/// Responsible for persisting parsed documents into PostgreSQL using EF Core.
/// Encapsulates all data-mapping logic between the Application layer and the Domain entities.
/// </summary>
public sealed class DocumentWriter : IDocumentWriter
{
    private readonly PurDbContext _db;

    /// <summary>
    /// Creates a new instance of <see cref="DocumentWriter"/> using dependency injection.
    /// </summary>
    /// <param name="db">EF Core database context for persistence.</param>
    public DocumentWriter(PurDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<SaveSummary> SaveAsync(IEnumerable<Document> documents, CancellationToken ct)
    {
        if (documents is null)
            throw new ArgumentNullException(nameof(documents));

        // Use a single transaction for atomic persistence of the entire batch.
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var mappedDocs = new List<Dom.Document>();
        var mappedItems = new List<Dom.DocumentItem>();

        foreach (var d in documents)
        {
            var docEntity = new Dom.Document
            {
                Id = Guid.NewGuid(),
                BaCode = d.BaCode,
                Type = d.Type,
                DocumentNumber = d.DocumentNumber,
                OperationDate = d.OperationDate,
                DocumentDayNumber = d.DocumentDayNumber,
                ContractorCode = d.ContractorCode,
                ContractorName = d.ContractorName,
                ExternalDocumentNumber = d.ExternalDocumentNumber,
                ExternalDocumentDate = d.ExternalDocumentDate,
                NetTotal = d.NetTotal,
                VatTotal = d.VatTotal,
                GrossTotal = d.GrossTotal,
                Flag1 = d.Flag1,
                Flag2 = d.Flag2,
                Flag3 = d.Flag3,
                Comment = d.Comment
            };

            mappedDocs.Add(docEntity);

            if (d.Items is null)
                continue;

            foreach (var i in d.Items)
            {
                var itemEntity = new Dom.DocumentItem
                {
                    Id = Guid.NewGuid(),
                    DocumentId = docEntity.Id,
                    ProductCode = i.ProductCode,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPriceNet = i.UnitPriceNet,
                    NetValue = i.NetValue,
                    VatValue = i.VatValue,
                    QuantityBefore = i.QuantityBefore,
                    AverageBefore = i.AverageBefore,
                    QuantityAfter = i.QuantityAfter,
                    AverageAfter = i.AverageAfter,
                    ProductGroup = i.ProductGroup
                };

                mappedItems.Add(itemEntity);
            }
        }

        // Efficient bulk insert in a single SaveChangesAsync call.
        await _db.Documents.AddRangeAsync(mappedDocs, ct);
        await _db.DocumentItems.AddRangeAsync(mappedItems, ct);
        await _db.SaveChangesAsync(ct);

        // Commit the transaction to ensure all records are persisted.
        await tx.CommitAsync(ct);

        return new SaveSummary(mappedDocs.Count, mappedItems.Count);
    }
}
