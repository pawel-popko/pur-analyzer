using System.Globalization;
using PurAnalyzer.Application.Parsing;

namespace PurAnalyzer.Infrastructure.Parsing;

/// <summary>
/// Lightweight .PUR parser with delimiter detection and basic date normalization.
/// </summary>
public sealed class PurFileParser : IPurFileParser
{
    public async Task<ParsedContent> ParseAsync(Stream stream)
    {
        var content = await PurParsingHelpers.ReadAllTextAsync(stream);
        var lines = PurParsingHelpers.SplitLines(content);

        // ── Validation and diagnostics
        var issues = new List<string>();
        var sawHeader = false;
        var sawBody = false;

        // ── Current header accumulator (H)
        string? baCode = null;
        string? type = null;
        string? documentNumber = null;
        DateTime? operationDate = null;
        int? documentDayNumber = null;
        string? contractorCode = null;
        string? contractorName = null;
        string? externalDocumentNumber = null;
        DateTime? externalDocumentDate = null;
        decimal? netTotal = null;
        decimal? vatTotal = null;
        decimal? grossTotal = null;
        string? flag1 = null, flag2 = null, flag3 = null;

        // ── Optional comment (C) associated with the current document
        string? currentComment = null;

        // ── Last header line number (for validation of "H after H without B")
        int? currentHeaderLineNo = null;

        var currentItems = new List<DocumentItem>();
        var documents = new List<Document>();

        bool HasAnyHeaderSet() =>
            baCode is not null || type is not null || documentNumber is not null ||
            operationDate is not null || documentDayNumber is not null ||
            contractorCode is not null || contractorName is not null ||
            externalDocumentNumber is not null || externalDocumentDate is not null ||
            netTotal is not null || vatTotal is not null || grossTotal is not null ||
            flag1 is not null || flag2 is not null || flag3 is not null;

        void FlushCurrent()
        {
            var hasAnyHeader = HasAnyHeaderSet();

            if (!hasAnyHeader && currentItems.Count == 0 && string.IsNullOrWhiteSpace(currentComment))
                return;

            documents.Add(new Document
            {
                BaCode = baCode,
                Type = type,
                DocumentNumber = documentNumber,
                OperationDate = operationDate,
                DocumentDayNumber = documentDayNumber,
                ContractorCode = contractorCode,
                ContractorName = contractorName,
                ExternalDocumentNumber = externalDocumentNumber,
                ExternalDocumentDate = externalDocumentDate,
                NetTotal = netTotal,
                VatTotal = vatTotal,
                GrossTotal = grossTotal,
                Flag1 = flag1,
                Flag2 = flag2,
                Flag3 = flag3,
                Comment = currentComment,
                Items = currentItems.ToList()
            });

            // Reset accumulators
            baCode = type = documentNumber = contractorCode = contractorName =
                externalDocumentNumber = flag1 = flag2 = flag3 = null;

            operationDate = null;
            documentDayNumber = null;
            externalDocumentDate = null;
            netTotal = vatTotal = grossTotal = null;
            currentItems.Clear();
            currentComment = null;
        }

        var lineNo = 0;

        foreach (var raw in lines)
        {
            lineNo++;
            var line = raw.Trim();
            if (line.Length == 0) continue;

            var sep = PurParsingHelpers.DetectSeparator(line);
            var parts = line.Split(sep);
            var rowType = parts.ElementAtOrDefault(0)?.Trim().ToUpperInvariant();

            if (rowType == "C")
            {
                // Comment line
                if (sawHeader)
                {
                    var commentText = string.Join(sep, parts.Skip(1)).Trim();

                    if (string.IsNullOrWhiteSpace(currentComment))
                    {
                        currentComment = string.IsNullOrWhiteSpace(commentText) ? null : commentText;
                    }
                    else
                    {
                        issues.Add($"Line {lineNo}: multiple comment rows (C) for a single document are not allowed.");
                    }
                }
                continue;
            }

            if (PurParsingHelpers.IsHeader(parts))
            {
                // New header row (H)
                if (currentHeaderLineNo is not null && currentItems.Count == 0 && HasAnyHeaderSet())
                {
                    issues.Add($"Header at line {lineNo} follows header at line {currentHeaderLineNo} without any item rows (B) in between.");
                }

                sawHeader = true;

                if (parts.Length < 16)
                    issues.Add($"Header at line {lineNo}: expected at least 16 columns, got {parts.Length}.");

                // Close previous document
                FlushCurrent();

                // Header mapping (H):
                // 0: H
                // 1: BA code, 2: Type, 3: Document number,
                // 4: Operation date, 5: Document day number,
                // 6: Contractor code, 7: Contractor name,
                // 8: External doc no., 9: External doc date,
                // 10: Net total, 11: VAT total, 12: Gross total,
                // 13: Flag1, 14: Flag2, 15: Flag3

                baCode = parts.ElementAtOrDefault(1);
                type = parts.ElementAtOrDefault(2);
                documentNumber = parts.ElementAtOrDefault(3);

                var rawOpDate = PurParsingHelpers.NormalizeDate(parts.ElementAtOrDefault(4));
                var rawExtDt = PurParsingHelpers.NormalizeDate(parts.ElementAtOrDefault(9));

                operationDate = ParseIsoDate(rawOpDate);
                documentDayNumber = ParseInt(parts.ElementAtOrDefault(5));

                if (!string.IsNullOrWhiteSpace(rawOpDate) && operationDate is null)
                    issues.Add($"Header at line {lineNo}: invalid operation date '{parts.ElementAtOrDefault(4)}'.");

                contractorCode = parts.ElementAtOrDefault(6);
                contractorName = parts.ElementAtOrDefault(7);

                externalDocumentNumber = parts.ElementAtOrDefault(8);
                externalDocumentDate = ParseIsoDate(rawExtDt);

                if (!string.IsNullOrWhiteSpace(rawExtDt) && externalDocumentDate is null)
                    issues.Add($"Header at line {lineNo}: invalid external document date '{parts.ElementAtOrDefault(9)}'.");

                netTotal = PurParsingHelpers.ParseDec(parts.ElementAtOrDefault(10));
                vatTotal = PurParsingHelpers.ParseDec(parts.ElementAtOrDefault(11));
                grossTotal = PurParsingHelpers.ParseDec(parts.ElementAtOrDefault(12));

                flag1 = parts.ElementAtOrDefault(13);
                flag2 = parts.ElementAtOrDefault(14);
                flag3 = parts.ElementAtOrDefault(15);

                currentComment = null;        // new document → reset comment
                currentHeaderLineNo = lineNo; // remember H
            }
            else if (PurParsingHelpers.IsBody(parts))
            {
                // Body row (B)
                sawBody = true;

                if (parts.Length < 12)
                    issues.Add($"Item at line {lineNo}: expected at least 12 columns, got {parts.Length}.");

                currentItems.Add(new DocumentItem
                {
                    ProductCode = parts.ElementAtOrDefault(1) ?? string.Empty,
                    ProductName = parts.ElementAtOrDefault(2) ?? string.Empty,
                    Quantity = PurParsingHelpers.ParseDec(parts.ElementAtOrDefault(3)),
                    UnitPriceNet = PurParsingHelpers.ParseDec(parts.ElementAtOrDefault(4)),
                    NetValue = PurParsingHelpers.ParseDec(parts.ElementAtOrDefault(5)),
                    VatValue = PurParsingHelpers.ParseDec(parts.ElementAtOrDefault(6)),
                    QuantityBefore = PurParsingHelpers.ParseDec(parts.ElementAtOrDefault(7)),
                    AverageBefore = PurParsingHelpers.ParseDec(parts.ElementAtOrDefault(8)),
                    QuantityAfter = PurParsingHelpers.ParseDec(parts.ElementAtOrDefault(9)),
                    AverageAfter = PurParsingHelpers.ParseDec(parts.ElementAtOrDefault(10)),
                    ProductGroup = parts.ElementAtOrDefault(11)
                });
            }
            else
            {
                // Unknown row type (e.g., "D")
                issues.Add($"Line {lineNo}: unexpected row type '{rowType ?? "?"}' (allowed: H, B, C).");
            }
        }

        // ── End of file: if last H had no B rows, also report it
        if (currentHeaderLineNo is not null && currentItems.Count == 0 && HasAnyHeaderSet())
        {
            issues.Add($"Header at line {currentHeaderLineNo} has no item rows (B).");
        }

        // Flush last document (controller may still return 422)
        FlushCurrent();

        // Basic structural validation
        if (!sawHeader) issues.Add("Missing header row (H).");
        if (!sawBody) issues.Add("No item rows (B) found.");

        var isValid = issues.Count == 0;

        return new ParsedContent
        {
            Lines = lines,
            Documents = documents,
            CharCount = content.Length,
            LineCount = lines.Count,
            PositionsCount = documents.Sum(d => d.Items?.Count ?? 0),
            IsFormatValid = isValid,
            FormatIssues = issues
        };
    }

    private static DateTime? ParseIsoDate(string? normalized)
    {
        if (string.IsNullOrWhiteSpace(normalized)) return null;

        if (DateTime.TryParseExact(
            normalized,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var dt))
        {
            return dt;
        }

        return DateTime.TryParse(normalized, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
            ? dt
            : null;
    }

    private static int? ParseInt(string? s)
        => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;
}
