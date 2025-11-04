using PurAnalyzer.Application.Analysis;
using PurAnalyzer.Application.Parsing;

namespace PurAnalyzer.Tests.ApplicationLayer.Analysis
{
    [TestFixture]
    public class PurFileAnalyzerTests
    {
        private static DocumentItem Item(string code, decimal net, string? name = null)
            => new DocumentItem { ProductCode = code, NetValue = net, ProductName = name };

        private static Document Doc(params DocumentItem[] items)
            => new Document { Items = items };

        [Test]
        public void Analyze_EmptyDocuments_ReturnsZeroCountsAndNoMessage()
        {
            // Arrange
            var sut = new PurFileAnalyzer();
            var docs = new List<Document>();
            var shape = new FileShape(false, false, false);

            // Act
            var result = sut.Analyze(docs, lineCount: 0, charCount: 0, x: 3, shape);

            // Assert
            Assert.That(result.Documents, Is.Empty, "Documents should be empty.");
            Assert.That(result.PositionsCount, Is.EqualTo(0), "No items => 0 positions.");
            Assert.That(result.XCount, Is.EqualTo(0), "No documents => 0 xCount.");
            Assert.That(result.ProductsWithMaxNetValue, Is.EqualTo(string.Empty), "No products => empty winners.");
            Assert.That(result.Message, Is.Null, "No shape rule should trigger.");
        }

        [Test]
        public void Analyze_MixedDocuments_ComputesPositionsCountAndXCount()
        {
            // Arrange
            // doc1 has 2 items, doc2 has 1 item, doc3 has 5 items
            var docs = new List<Document>
            {
                Doc(Item("A", 1m), Item("B", 2m)),
                Doc(Item("C", 3m)),
                Doc(Item("D", 4m), Item("E", 5m), Item("F", 6m), Item("G", 7m), Item("H", 8m))
            };
            var sut = new PurFileAnalyzer();
            var shape = new FileShape(true, true, false);
            const int x = 2;

            // Act
            var result = sut.Analyze(docs, lineCount: 10, charCount: 100, x, shape);

            // Assert
            Assert.That(result.PositionsCount, Is.EqualTo(8), "Total items should be summed across all documents.");
            Assert.That(result.XCount, Is.EqualTo(1), "Only doc3 has more than x=2 items.");
            Assert.That(result.LineCount, Is.EqualTo(10));
            Assert.That(result.CharCount, Is.EqualTo(100));
            Assert.That(result.Documents.Count(), Is.EqualTo(3));
        }

        [Test]
        public void Analyze_SingleMaxNetWinner_UsesMostFrequentNonEmptyName()
        {
            // Arrange
            // Code X total net = 30 (wins). Names appear with different frequencies.
            var doc = Doc(
                Item("X", 10m, "Alpha"),
                Item("X", 10m, "alpha"), // same name diff case – should be grouped case-insensitively
                Item("X", 10m, "ALPHA"),
                Item("Y", 5m, "Bravo")
            );
            var sut = new PurFileAnalyzer();

            // Act
            var result = sut.Analyze(new[] { doc }, lineCount: 1, charCount: 1, x: 0,
                new FileShape(true, true, false));

            // Assert
            Assert.That(result.ProductsWithMaxNetValue, Is.EqualTo("Alpha"),
                "Winner should be the most frequent non-empty name (case-insensitive).");
        }

        [Test]
        public void Analyze_SingleMaxNetWinner_FallsBackToCodeWhenNoName()
        {
            // Arrange
            var doc = Doc(
                Item("Z1", 12m, ""),
                Item("Z1", 8m, null),
                Item("Y1", 15m, "Bravo")
            );
            var sut = new PurFileAnalyzer();

            // Act
            var result = sut.Analyze(new[] { doc }, 0, 0, 0, new FileShape(true, true, false));

            // Assert
            Assert.That(result.ProductsWithMaxNetValue, Is.EqualTo("Z1"),
                "If all names are empty/whitespace, display code.");
        }

        [Test]
        public void Analyze_TiedMaxNetWinners_ReturnsDistinctCaseInsensitiveSortedNames()
        {
            // Arrange
            // Code A net=20, Code b net=20 -> tie, names deduped and sorted asc.
            var doc = Doc(
                Item("A", 5m, "Alpha"),
                Item("A", 15m, "ALPHA"),
                Item("b", 12m, "beta"),
                Item("B", 8m, "Beta")
            );
            var sut = new PurFileAnalyzer();

            // Act
            var result = sut.Analyze(new[] { doc }, 0, 0, 0, new FileShape(true, true, false));

            // Assert
            Assert.That(result.ProductsWithMaxNetValue, Is.EqualTo("Alpha, Beta").IgnoreCase,
                "Winners should be distinct (case-insensitive) and sorted alphabetically.");
        }

        [Test]
        public void Analyze_ItemRowsWithoutHeader_SetsInvalidFormatMessage()
        {
            // Arrange
            var sut = new PurFileAnalyzer();
            var docs = new[] { Doc(Item("A", 1m)) };
            var shape = new FileShape(false, true, false);

            // Act
            var result = sut.Analyze(docs, 0, 0, 0, shape);

            // Assert
            Assert.That(result.Message, Is.EqualTo("Invalid file format – item rows (B) present without any header (H)."));
        }

        [Test]
        public void Analyze_OnlyComments_SetsOnlyCommentsMessage()
        {
            // Arrange
            var sut = new PurFileAnalyzer();
            var docs = new List<Document>(); // no documents/items
            var shape = new FileShape(false, false, true);

            // Act
            var result = sut.Analyze(docs, 0, 0, 0, shape);

            // Assert
            Assert.That(result.Message, Is.EqualTo("File contains only comments (C)."));
        }

        [Test]
        public void Analyze_HeaderWithoutItems_SetsNoItemRowsMessage()
        {
            // Arrange
            var sut = new PurFileAnalyzer();
            var docs = new[] { new Document { Items = Array.Empty<DocumentItem>() } };
            var shape = new FileShape(true, false, false);

            // Act
            var result = sut.Analyze(docs, 0, 0, 0, shape);

            // Assert
            Assert.That(result.PositionsCount, Is.EqualTo(0));
            Assert.That(result.Message, Is.EqualTo("No item rows (B) found for any document."));
        }

        // --- Extra tests to improve branch coverage ---

        [Test]
        public void Analyze_SameCodeEqualNameFrequency_PicksAlphabeticallyFirstName()
        {
            // Arrange: for code X we have two names with the same frequency: "Alpha" x2, "Beta" x2
            // Equal frequency should trigger ThenBy(...) and pick "Alpha" (alphabetically first).
            var doc = Doc(
                Item("X", 5m, "alpha"),
                Item("X", 5m, "ALPHA"),
                Item("X", 5m, "beta"),
                Item("X", 5m, "BETA")
            );
            var sut = new PurFileAnalyzer();

            // Act
            var result = sut.Analyze(new[] { doc }, 0, 0, 0, new FileShape(true, true, false));

            // Assert
            Assert.That(result.ProductsWithMaxNetValue, Is.EqualTo("Alpha"),
                "With equal frequencies the analyzer should choose the alphabetically first normalized name.");
        }

        [Test]
        public void Analyze_TiedWinnersWithCodesOnly_UppercasesFirstLetterOnFallback()
        {
            // Arrange: two winners with the same total net, names missing -> fallback to code (lowercase).
            var doc = Doc(
                Item("a1", 10m, null),
                Item("b1", 10m, ""),
                Item("c9", 3m, "ignored")
            );
            var sut = new PurFileAnalyzer();

            // Act
            var result = sut.Analyze(new[] { doc }, 0, 0, 0, new FileShape(true, true, false));

            // Assert: distinct, sorted, and first letter uppercased by UppercaseFirst
            Assert.That(result.ProductsWithMaxNetValue, Is.EqualTo("A1, B1"));
        }

        [Test]
        public void Analyze_WinnerHasEmptyCodeAndNoName_CoversUppercaseFirstWhitespaceBranch()
        {
            // Arrange
            // The winning group has a null ProductCode (normalized to empty string) and all names are null or whitespace.
            // This scenario triggers UppercaseFirst(...) with an empty string, covering the whitespace branch.
            var doc = Doc(
                new DocumentItem { ProductCode = null, NetValue = 10m, ProductName = "   " },
                new DocumentItem { ProductCode = null, NetValue = 0m, ProductName = null },
                new DocumentItem { ProductCode = "X1", NetValue = 9m, ProductName = "X" }
            );

            var sut = new PurFileAnalyzer();

            // Act
            var result = sut.Analyze(
                new[] { doc },
                lineCount: 0,
                charCount: 0,
                x: 0,
                new FileShape(hasH: true, hasB: true, hasC: false)
            );

            // Assert
            // Since the winner's name and code are empty, ProductsWithMaxNetValue should be an empty string.
            // This ensures the branch in UppercaseFirst (string.IsNullOrWhiteSpace(s)) is executed.
            Assert.That(result.ProductsWithMaxNetValue, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Analyze_NullItemsOnSomeDocuments_TreatsAsZeroAndDoesNotAffectXCount()
        {
            // Arrange
            // This setup includes documents with null Items to exercise the null-coalescing branch:
            // - doc1.Items == null        -> contributes 0 positions
            // - doc2.Items has 2 elements -> contributes 2 positions and should count to XCount when x=1
            // - doc3.Items == null        -> contributes 0 positions
            var docs = new List<Document>
    {
        new Document { Items = null },
        new Document { Items = new[] { new DocumentItem { ProductCode = "A", NetValue = 1m },
                                       new DocumentItem { ProductCode = "B", NetValue = 2m } } },
        new Document { Items = null }
    };

            var sut = new PurFileAnalyzer();
            var shape = new FileShape(hasH: true, hasB: true, hasC: false);
            const int x = 1;

            // Act
            var result = sut.Analyze(docs, lineCount: 0, charCount: 0, x, shape);

            // Assert
            // positionsCount should sum only non-null item collections (2 total).
            Assert.That(result.PositionsCount, Is.EqualTo(2), "Null Items should be treated as zero-length collections.");
            // XCount should consider only documents with item count > x (only the second doc: 2 > 1).
            Assert.That(result.XCount, Is.EqualTo(1), "Documents with null Items must not contribute to XCount.");
        }
    }
}
