using PurAnalyzer.Application.Parsing;

namespace PurAnalyzer.Application.Analysis;

/// <summary>
/// Computes all required metrics based on parsed documents and the route threshold 'x'.
/// Keeps business rules centralized and testable.
/// </summary>
public sealed class PurFileAnalyzer : IPurFileAnalyzer
{
    /// <inheritdoc />
    public AnalysisResult Analyze(
        IReadOnlyList<Document> documents,
        int lineCount,
        int charCount,
        int x,
        FileShape shape)
    {
        var positionsCount = documents.Sum(d => d.Items?.Count() ?? 0);
        var xCount = documents.Count(d => (d.Items?.Count() ?? 0) > x);

        var allItems = documents.SelectMany(d => d.Items ?? Array.Empty<DocumentItem>()).ToList();

        var byCode = allItems
            .GroupBy(i => i.ProductCode ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .Select(g => new
            {
                Code = g.Key,
                Net = g.Sum(i => i.NetValue),
                Names = g.Select(i => i.ProductName ?? string.Empty)
            })
            .ToList();

        string productsWithMax = string.Empty;

        if (byCode.Count > 0)
        {
            var maxNet = byCode.Max(x => x.Net);
            var winners = byCode.Where(x => x.Net == maxNet).ToList();

            var displayNames = winners.Select(w =>
            {
                var bestName = w.Names
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .GroupBy(n => n, StringComparer.OrdinalIgnoreCase)
                    .OrderByDescending(g => g.Count())
                    .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.Key)
                    .FirstOrDefault();

                return string.IsNullOrWhiteSpace(bestName) ? w.Code : bestName;
            });

            productsWithMax = string.Join(", ",
                displayNames
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(UppercaseFirst)
                    .OrderBy(n => n, StringComparer.OrdinalIgnoreCase));

        }

        // --- messages
        string? message = null;

        if (!shape.HasH && shape.HasB)
        {
            message = "Invalid file format – item rows (B) present without any header (H).";
        }
        else if (shape.HasC && !shape.HasH && !shape.HasB)
        {
            message = "File contains only comments (C).";
        }
        else if (shape.HasH && positionsCount == 0)
        {
            message = "No item rows (B) found for any document.";
        }

        return new AnalysisResult
        {
            Documents = documents,
            LineCount = lineCount,
            CharCount = charCount,
            PositionsCount = positionsCount,
            XCount = xCount,
            ProductsWithMaxNetValue = productsWithMax,
            Message = message
        };
    }

    private static string UppercaseFirst(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return s;
        return char.ToUpperInvariant(s[0]) + (s.Length > 1 ? s.Substring(1) : string.Empty);
    }

}
