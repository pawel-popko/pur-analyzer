using PurAnalyzer.Infrastructure.Parsing;

namespace PurAnalyzer.Tests.InfrastructureLayer.Parsing;

[TestFixture]
public class FixPolishArtifactsTests
{
    [Test]
    public void FixPolishArtifacts_ShouldReplaceAllMappedGlyphs_WhenInputContainsArtifacts()
    {
        // Arrange: same keys as in PolishFixMap
        var input = "˝ ť Ť ŕ Ă Â Ş Ţ ă Ľ Ĺ";
        var expected = "Ż Ł Ł Ó Ą Ć Ś Ź Ń Ł Ł";

        // Act
        var result = PurParsingHelpers.FixPolishArtifacts(input);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void FixPolishArtifacts_ShouldReturnInput_WhenNullOrEmpty()
    {
        Assert.That(PurParsingHelpers.FixPolishArtifacts(""), Is.EqualTo(""));
    }
}
