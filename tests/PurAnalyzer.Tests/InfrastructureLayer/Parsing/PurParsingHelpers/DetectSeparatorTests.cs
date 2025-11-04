using PurAnalyzer.Infrastructure.Parsing;

namespace PurAnalyzer.Tests.InfrastructureLayer.Parsing;

[TestFixture]
public class DetectSeparatorTests
{
    [Test]
    public void DetectSeparator_ShouldReturnComma_WhenOnlyCommasPresent()
    {
        // Arrange
        var line = "A,B,C,D";

        // Act
        var result = PurParsingHelpers.DetectSeparator(line);

        // Assert
        Assert.That(result, Is.EqualTo(','));
    }

    [Test]
    public void DetectSeparator_ShouldReturnSemicolon_WhenOnlySemicolonsPresent()
    {
        // Arrange
        var line = "A;B;C;D";

        // Act
        var result = PurParsingHelpers.DetectSeparator(line);

        // Assert
        Assert.That(result, Is.EqualTo(';'));
    }

    [Test]
    public void DetectSeparator_ShouldPreferSemicolon_WhenBothPresentAndSemicolonMoreFrequent()
    {
        // Arrange
        var line = "A;B;C,D;E";

        // Act
        var result = PurParsingHelpers.DetectSeparator(line);

        // Assert
        Assert.That(result, Is.EqualTo(';'));
    }

    [Test]
    public void DetectSeparator_ShouldPreferComma_WhenBothPresentAndCommaMoreFrequent()
    {
        // Arrange
        var line = "A,B;C,D,E";

        // Act
        var result = PurParsingHelpers.DetectSeparator(line);

        // Assert
        Assert.That(result, Is.EqualTo(','));
    }

    [Test]
    public void DetectSeparator_ShouldReturnSemicolon_WhenCountsAreEqual()
    {
        // Arrange
        var line = "A,B;C;D,E"; // 2 commas, 2 semicolons

        // Act
        var result = PurParsingHelpers.DetectSeparator(line);

        // Assert
        Assert.That(result, Is.EqualTo(';'), "Equal counts should default to semicolon");
    }
}
