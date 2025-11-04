using PurAnalyzer.Infrastructure.Parsing;

namespace PurAnalyzer.Tests.InfrastructureLayer.Parsing;

[TestFixture]
public class ParseDecTests
{
    [TestCase("123", 123)]
    [TestCase("123.45", 123.45)]
    [TestCase("  42  ", 42)]
    [TestCase("1,000", 1000)] // comma as thousands separator in InvariantCulture
    public void ParseDec_ShouldParseValidInvariantStrings(string input, decimal expected)
    {
        // Act
        var result = PurParsingHelpers.ParseDec(input);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("bad")]
    [TestCase("1_000")] // underscore not allowed with InvariantCulture
    public void ParseDec_ShouldReturnZero_OnNullEmptyOrInvalid(string? input)
    {
        // Act
        var result = PurParsingHelpers.ParseDec(input);

        // Assert
        Assert.That(result, Is.EqualTo(0m));
    }
}
