using Microsoft.EntityFrameworkCore;
using PurAnalyzer.Domain.Parsing;

namespace PurAnalyzer.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for persistence.
/// </summary>
public sealed class PurDbContext(DbContextOptions<PurDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentItem> DocumentItems => Set<DocumentItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply IEntityTypeConfiguration<> from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PurDbContext).Assembly);
    }
}
