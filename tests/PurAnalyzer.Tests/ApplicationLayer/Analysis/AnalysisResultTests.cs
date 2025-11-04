using PurAnalyzer.Application.Analysis;
using PurAnalyzer.Application.Parsing;

namespace PurAnalyzer.Tests.ApplicationLayer.Analysis
{
    [TestFixture]
    public class AnalysisResultTests
    {
        [Test]
        public void AnalysisResult_DefaultValues_AreExpected()
        {
            // Act
            var result = new AnalysisResult();

            // Assert
            Assert.That(result.Documents, Is.Not.Null, "Documents should be a non-null empty sequence by default.");
            Assert.That(result.Documents.Count(), Is.EqualTo(0), "Documents should be empty by default.");
            Assert.That(result.LineCount, Is.EqualTo(0), "LineCount should default to 0.");
            Assert.That(result.CharCount, Is.EqualTo(0), "CharCount should default to 0.");
            Assert.That(result.PositionsCount, Is.EqualTo(0), "PositionsCount should default to 0.");
            Assert.That(result.XCount, Is.EqualTo(0), "XCount should default to 0.");
            Assert.That(result.ProductsWithMaxNetValue, Is.EqualTo(string.Empty), "ProductsWithMaxNetValue should default to empty string.");
            Assert.That(result.Message, Is.Null, "Message should default to null.");
        }

        [Test]
        public void AnalysisResult_CustomInitialization_AssignsValues()
        {
            // Arrange
            // Using an array of Document to verify assignment (no need to construct items).
            var docs = new Document[2];

            // Act
            var result = new AnalysisResult
            {
                Documents = docs,
                LineCount = 123,
                CharCount = 4567,
                PositionsCount = 3,
                XCount = 9,
                ProductsWithMaxNetValue = "PROD-A, PROD-B",
                Message = "OK"
            };

            // Assert
            Assert.That(result.Documents, Is.SameAs(docs), "Documents reference should be preserved.");
            Assert.That(result.Documents.Count(), Is.EqualTo(2), "Documents length should match the provided array.");
            Assert.That(result.LineCount, Is.EqualTo(123));
            Assert.That(result.CharCount, Is.EqualTo(4567));
            Assert.That(result.PositionsCount, Is.EqualTo(3));
            Assert.That(result.XCount, Is.EqualTo(9));
            Assert.That(result.ProductsWithMaxNetValue, Is.EqualTo("PROD-A, PROD-B"));
            Assert.That(result.Message, Is.EqualTo("OK"));
        }

        [Test]
        public void AnalysisResult_Type_IsSealed()
        {
            // Assert
            Assert.That(typeof(AnalysisResult).IsSealed, Is.True, "AnalysisResult should be sealed.");
        }
    }
}
