using PurAnalyzer.Application.Parsing;

namespace PurAnalyzer.Application.Analysis;

/// <summary>
/// Defines an abstraction for persisting analyzed documents into a relational database.
/// Keeps the API layer decoupled from Entity Framework or any other specific data store.
/// </summary>
public interface IDocumentWriter
{
    /// <summary>
    /// Persists a collection of parsed documents and their related items into the database.
    /// </summary>
    /// <param name="documents">The parsed documents to be stored.</param>
    /// <param name="ct">A cancellation token for graceful termination.</param>
    /// <returns>
    /// A summary describing how many documents and items were successfully persisted.
    /// </returns>
    Task<SaveSummary> SaveAsync(IEnumerable<Document> documents, CancellationToken ct);
}

/// <summary>
/// A lightweight summary describing the outcome of a save operation.
/// </summary>
/// <param name="Documents">The number of top-level documents saved.</param>
/// <param name="Items">The number of document items saved.</param>
public sealed record SaveSummary(int Documents, int Items);
