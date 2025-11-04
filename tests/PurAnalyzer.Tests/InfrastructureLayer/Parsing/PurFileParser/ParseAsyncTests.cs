using System.Text;
using PurAnalyzer.Infrastructure.Parsing;

namespace PurAnalyzer.Tests.InfrastructureLayer.Parsing;

[TestFixture]
public class ParseAsyncTests
{
    private static MemoryStream ToStream(string text)
        => new MemoryStream(Encoding.UTF8.GetBytes(text));

    private static string Header(
        string ba = "5309", string type = "01", string no = "00125",
        string opDate = "2015-01-29", string dayNo = "5190",
        string contractorCode = "627", string contractorName = "BONA7 SP. Z O.O.",
        string extNo = "WZ-8111/1/BS/15", string extDate = "2015-01-29",
        string net = "20.39", string vat = "1.63", string gross = "22.02",
        string f1 = "0.00", string f2 = "0.00", string f3 = "0.00",
        char sep = ';')
        => string.Join(sep, new[]
        {
            "H", ba, type, no, opDate, dayNo, contractorCode, contractorName,
            extNo, extDate, net, vat, gross, f1, f2, f3
        });

    private static string Body(
        string code = "06336", string name = "TELE TYDZIEŃ", string qty = "2",
        string price = "1.48", string net = "2.96", string vat = "0.23",
        string qBefore = "0", string avgBefore = "0", string qAfter = "2",
        string avgAfter = "1.48", string group = "MAG", char sep = ';')
        => string.Join(sep, new[]
        {
            "B", code, name, qty, price, net, vat, qBefore, avgBefore, qAfter, avgAfter, group
        });

    [Test]
    public async Task ParseAsync_HappyPath_ParsesSingleDocumentWithComment()
    {
        // Arrange
        var text = string.Join('\n', new[]
        {
            Header(),
            Body(),
            "C;Dostawa poranna"
        });
        var parser = new PurFileParser();

        // Act
        var result = await parser.ParseAsync(ToStream(text));

        // Assert
        Assert.That(result.IsFormatValid, Is.True, string.Join(" | ", result.FormatIssues));
        Assert.That(result.Documents, Has.Count.EqualTo(1));
        var doc = result.Documents[0];
        Assert.That(doc.Items, Has.Count.EqualTo(1));
        Assert.That(doc.Comment, Is.EqualTo("Dostawa poranna"));
        Assert.That(doc.BaCode, Is.EqualTo("5309"));
        Assert.That(doc.DocumentNumber, Is.EqualTo("00125"));
        Assert.That(doc.OperationDate?.ToString("yyyy-MM-dd"), Is.EqualTo("2015-01-29"));
        Assert.That(result.PositionsCount, Is.EqualTo(1));
    }

    [Test]
    public async Task ParseAsync_HeaderWithoutBody_AddsIssueAndFlushesDocument()
    {
        // Arrange
        var text = Header(); // no body rows
        var parser = new PurFileParser();

        // Act
        var result = await parser.ParseAsync(ToStream(text));

        // Assert
        Assert.That(result.IsFormatValid, Is.False);
        Assert.That(result.FormatIssues, Has.Some.Contains("has no item rows (B)"));
        Assert.That(result.Documents, Has.Count.EqualTo(1));
        Assert.That(result.Documents[0].Items, Is.Empty);
    }


    [Test]
    public async Task ParseAsync_HeaderFollowedByHeaderWithoutItems_ReportsIssue()
    {
        // Arrange
        var text = string.Join('\n', new[]
        {
        Header(),            // Header #1 without items
        Header(no:"00126"),  // Header #2
        Body(name:"INNA NAZWA") // One item for the second document
    });
        var parser = new PurFileParser();

        // Act
        var result = await parser.ParseAsync(ToStream(text));

        // Assert
        Assert.That(result.IsFormatValid, Is.False);
        Assert.That(
            result.FormatIssues,
            Has.Some.Contains("follows header at line 1 without any item rows (B) in between.")
        );
        Assert.That(result.Documents.Last().Items, Has.Count.EqualTo(1));
    }


    [Test]
    public async Task ParseAsync_MultipleCommentRows_KeepsFirstAndReportsIssue()
    {
        // Arrange
        var text = string.Join('\n', new[]
        {
            Header(),
            Body(),
            "C;Pierwszy komentarz",
            "C;Drugi komentarz"
        });
        var parser = new PurFileParser();

        // Act
        var result = await parser.ParseAsync(ToStream(text));

        // Assert
        Assert.That(result.IsFormatValid, Is.False);
        Assert.That(result.FormatIssues, Has.Some.Contains("multiple comment rows (C)"));
        Assert.That(result.Documents[0].Comment, Is.EqualTo("Pierwszy komentarz"));
    }

    [Test]
    public async Task ParseAsync_UnknownRowType_AddsIssue()
    {
        // Arrange
        var text = string.Join('\n', new[]
        {
            Header(),
            "D;coś;dziwnego",
            Body()
        });
        var parser = new PurFileParser();

        // Act
        var result = await parser.ParseAsync(ToStream(text));

        // Assert
        Assert.That(result.IsFormatValid, Is.False);
        Assert.That(result.FormatIssues, Has.Some.Contains("unexpected row type 'D'"));
    }

    [Test]
    public async Task ParseAsync_TooFewColumnsInHeaderAndBody_ReportColumnCountIssues()
    {
        // Arrange
        var badHeader = "H;5309;01;00125;2015-01-29"; // only 5 columns instead of >=16
        var badBody = "B;06336;NAZWA;2;1.48";         // only 5 columns instead of >=12
        var text = string.Join('\n', new[] { badHeader, badBody });
        var parser = new PurFileParser();

        // Act
        var result = await parser.ParseAsync(ToStream(text));

        // Assert
        Assert.That(result.IsFormatValid, Is.False);
        Assert.That(result.FormatIssues, Has.Some.Contains("expected at least 16 columns"));
        Assert.That(result.FormatIssues, Has.Some.Contains("expected at least 12 columns"));
    }


    [Test]
    public async Task ParseAsync_CommentLineBeforeAnyHeader_IsIgnored()
    {
        // Arrange
        var text = string.Join('\n', new[]
        {
            "C;Ten komentarz powinien zostać zignorowany",
            Header(),
            Body()
        });
        var parser = new PurFileParser();

        // Act
        var result = await parser.ParseAsync(ToStream(text));

        // Assert
        Assert.That(result.IsFormatValid, Is.True, string.Join(" | ", result.FormatIssues));
        Assert.That(result.Documents[0].Comment, Is.Null);
    }

    [Test]
    public async Task ParseAsync_NoHeaderNoBody_ReportsMissingHeaderAndNoBody()
    {
        // Arrange
        var text = "C;Ten komentarz bez nagłówka i pozycji"; // only comment, no H, no B
        var parser = new PurFileParser();

        // Act
        var result = await parser.ParseAsync(ToStream(text));

        // Assert
        Assert.That(result.IsFormatValid, Is.False);
        Assert.That(result.Documents, Is.Empty);
        Assert.That(result.FormatIssues, Has.Some.Contains("Missing header row (H)."));
        Assert.That(result.FormatIssues, Has.Some.Contains("No item rows (B) found."));
    }

    [Test]
    public async Task ParseAsync_BodyWithoutHeader_ReportsMissingHeaderOnly()
    {
        // Arrange
        var text = string.Join('\n', new[]
        {
            Body(name:"Produkt bez nagłówka") // B present, no H
        });
        var parser = new PurFileParser();

        // Act
        var result = await parser.ParseAsync(ToStream(text));

        // Assert
        Assert.That(result.IsFormatValid, Is.False);
        Assert.That(result.Documents, Has.Count.EqualTo(1)); // flushed at the end with one item
        Assert.That(result.FormatIssues, Has.Some.Contains("Missing header row (H)."));
        Assert.That(result.FormatIssues, Has.None.Contains("No item rows (B) found."));
    }

    [Test]
    public async Task ParseAsync_DatesWithTime_ParsedViaFallbackTryParse()
    {
        // Arrange
        // Use timestamps with time component to bypass TryParseExact("yyyy-MM-dd")
        var text = string.Join('\n', new[]
        {
        // opDate and extDate provided with time (ISO-like but not "yyyy-MM-dd")
        Header(opDate: "2015-01-29T00:00:00", extDate: "2015-01-29T12:34:56"),
        Body(name: "Produkt z datą z czasem")
    });
        var parser = new PurFileParser();

        // Act
        var result = await parser.ParseAsync(ToStream(text));

        // Assert
        Assert.That(result.IsFormatValid, Is.True, string.Join(" | ", result.FormatIssues));
        Assert.That(result.Documents, Has.Count.EqualTo(1));
        var doc = result.Documents[0];

        // Both dates should be parsed (fallback DateTime.TryParse branch)
        Assert.That(doc.OperationDate, Is.Not.Null);
        Assert.That(doc.ExternalDocumentDate, Is.Not.Null);
        Assert.That(doc.OperationDate?.ToString("yyyy-MM-dd"), Is.EqualTo("2015-01-29"));
        Assert.That(doc.ExternalDocumentDate?.ToString("yyyy-MM-dd"), Is.EqualTo("2015-01-29"));
    }

    [Test]
    public async Task ParseAsync_InvalidDates_ReportIssues_AndKeepDatesNull()
    {
        // Arrange
        // Provide clearly invalid dates so ParseIsoDate returns null and issues are added
        var text = string.Join('\n', new[]
        {
            Header(opDate: "2015-13-40", extDate: "abcd"), // impossible dates
            Body(name: "Produkt z błędną datą")
        });
        var parser = new PurFileParser();

        // Act
        var result = await parser.ParseAsync(ToStream(text));

        // Assert
        Assert.That(result.IsFormatValid, Is.False);
        Assert.That(result.Documents, Has.Count.EqualTo(1));
        var doc = result.Documents[0];

        // Dates should be null due to invalid format
        Assert.That(doc.OperationDate, Is.Null);
        Assert.That(doc.ExternalDocumentDate, Is.Null);

        // Issues should mention both invalid dates with original raw values
        Assert.That(result.FormatIssues, Has.Some.Contains("invalid operation date '2015-13-40'."));
        Assert.That(result.FormatIssues, Has.Some.Contains("invalid external document date 'abcd'."));
    }

    [Test]
    public async Task ParseAsync_InvalidOperationDate_NonEmptyAfterNormalization_AddsIssue()
    {
        // Arrange
        // Use an operation date that remains non-empty after normalization but cannot be parsed.
        // This should trigger: if (!string.IsNullOrWhiteSpace(rawOpDate) && operationDate is null) {...}  // line 159
        var text = string.Join('\n', new[]
        {
        Header(opDate: "2015-01-29Tbad", extDate: "2015-01-29"), // valid ext date to isolate the op date branch
        Body(name: "Any item")
    });
        var parser = new PurFileParser();

        // Act
        var result = await parser.ParseAsync(ToStream(text));

        // Assert
        Assert.That(result.IsFormatValid, Is.False);
        Assert.That(result.FormatIssues, Has.Some.Contains("invalid operation date '2015-01-29Tbad'."));
        // Sanity checks
        Assert.That(result.Documents, Has.Count.EqualTo(1));
        Assert.That(result.Documents[0].OperationDate, Is.Null);
        Assert.That(result.Documents[0].ExternalDocumentDate, Is.Not.Null);
    }

    [Test]
    public async Task ParseAsync_CommentLineWithWhitespace_SetsCommentToNull()
    {
        // Arrange
        var text = string.Join('\n', new[]
        {
            Header(),
            Body(),
            "C;   "  // whitespace-only comment
        });
        var parser = new PurFileParser();

        // Act
        var result = await parser.ParseAsync(ToStream(text));

        // Assert
        Assert.That(result.IsFormatValid, Is.True, string.Join(" | ", result.FormatIssues));
        Assert.That(result.Documents, Has.Count.EqualTo(1));
        Assert.That(result.Documents[0].Comment, Is.Null, "Whitespace-only comment should be normalized to null.");
    }

    [Test]
    public async Task ParseAsync_BodyWithEmptyAndNumericFields_ExercisesNumericZeroDefaults()
    {
        // Arrange
        // Keep 12 columns; leave selected numeric fields empty to exercise the initializer paths.
        // Current semantics: empty numeric fields are parsed as 0m (not null).
        var badNumericBody = "B;CODE;NAME;;1.48;;0.23;;;2;1.48;GRP";
        var text = string.Join('\n', new[] { Header(), badNumericBody });
        var parser = new PurFileParser();

        // Act
        var result = await parser.ParseAsync(ToStream(text))!;

        // Assert
        Assert.That(result.IsFormatValid, Is.True, string.Join(" | ", result.FormatIssues));

        // Use null-forgiving to silence NRT warnings in tests; the parser always sets these.
        var it = result.Documents!.Single().Items!.Single();

        // Empty numeric fields → default to 0m
        Assert.That(it.Quantity, Is.EqualTo(0m), "Empty Quantity should default to 0m.");
        Assert.That(it.NetValue, Is.EqualTo(0m), "Empty NetValue should default to 0m.");
        Assert.That(it.QuantityBefore, Is.EqualTo(0m), "Empty QuantityBefore should default to 0m.");
        Assert.That(it.AverageBefore, Is.EqualTo(0m), "Empty AverageBefore should default to 0m.");

        // Non-empty numeric fields parsed as given
        Assert.That(it.UnitPriceNet, Is.EqualTo(1.48m));
        Assert.That(it.VatValue, Is.EqualTo(0.23m));
        Assert.That(it.QuantityAfter, Is.EqualTo(2m));
        Assert.That(it.AverageAfter, Is.EqualTo(1.48m));

        // String fields preserved (with null-coalescing to empty in the parser)
        Assert.That(it.ProductCode, Is.EqualTo("CODE"));
        Assert.That(it.ProductName, Is.EqualTo("NAME"));
        Assert.That(it.ProductGroup, Is.EqualTo("GRP"));
    }

    [Test]
    public async Task ParseAsync_EmptyDates_TreatedAsNullWithoutParsing()
    {
        // Arrange
        // Empty operation and external document dates should be treated as null
        // without invoking TryParse or adding any format issues.
        var text = string.Join('\n', new[]
        {
            Header(opDate: "", extDate: ""),
            Body()
        });
        var parser = new PurFileParser();

        // Act
        var result = await parser.ParseAsync(ToStream(text));

        // Assert
        var doc = result.Documents.Single();
        Assert.That(doc.OperationDate, Is.Null);
        Assert.That(doc.ExternalDocumentDate, Is.Null);
        Assert.That(result.IsFormatValid, Is.True, string.Join(" | ", result.FormatIssues));
    }
}
