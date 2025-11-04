using PurAnalyzer.Infrastructure.Parsing;

namespace PurAnalyzer.Tests.InfrastructureLayer.Parsing;

[TestFixture]
public class NormalizeDateTests
{
    [TestCase("29-01-2015", "2015-01-29")]  // dd-MM-yyyy
    [TestCase("2015-01-29", "2015-01-29")]  // yyyy-MM-dd
    [TestCase("29.01.2015", "2015-01-29")]  // dd.MM.yyyy
    public void NormalizeDate_ShouldReturnIso_WhenInputIsRecognized(string input, string expected)
    {
        // Act
        var result = PurParsingHelpers.NormalizeDate(input);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void NormalizeDate_ShouldReturnEmpty_WhenNullOrWhitespace(string? input)
    {
        // Act
        var result = PurParsingHelpers.NormalizeDate(input);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void NormalizeDate_ShouldReturnIso_WhenGeneralParsingAcceptsInput()
    {
        // Arrange: this format is not in the exact formats list,
        // but DateTime.TryParse (lenient) will parse it.
        var input = "2015/01/29";

        // Act
        var result = PurParsingHelpers.NormalizeDate(input);

        // Assert
        Assert.That(result, Is.EqualTo("2015-01-29"));
    }

    [Test]
    public void NormalizeDate_ShouldReturnOriginal_WhenCompletelyUnrecognizedFormat()
    {
        // Arrange: this should not be parsed by TryParse nor TryParseExact
        var input = "2015_01_29";

        // Act
        var result = PurParsingHelpers.NormalizeDate(input);

        // Assert
        Assert.That(result, Is.EqualTo(input), "Unknown formats should be returned unchanged (fallback).");
    }
}
