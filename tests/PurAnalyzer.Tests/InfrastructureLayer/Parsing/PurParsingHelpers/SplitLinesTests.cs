using PurAnalyzer.Infrastructure.Parsing;

namespace PurAnalyzer.Tests.InfrastructureLayer.Parsing;

[TestFixture]
public class SplitLinesTests
{
    [Test]
    public void SplitLines_ShouldNormalizeCrLf_AndRemoveEmptyLines()
    {
        // Arrange
        var input = "A\r\nB\n\r\n\nC\r\n";

        // Act
        var lines = PurParsingHelpers.SplitLines(input);

        // Assert
        Assert.That(lines, Is.EquivalentTo(new[] { "A", "B", "C" }));
    }

    [Test]
    public void SplitLines_ShouldReturnEmptyList_ForNullOrEmpty()
    {
        // Act + Assert
        Assert.That(PurParsingHelpers.SplitLines(""), Is.Empty);
    }

    [Test]
    public void SplitLines_ShouldReturnSingleLine_WhenNoNewlineCharactersPresent()
    {
        // Arrange
        var input = "SingleLine";

        // Act
        var lines = PurParsingHelpers.SplitLines(input);

        // Assert
        Assert.That(lines, Has.Count.EqualTo(1));
        Assert.That(lines[0], Is.EqualTo("SingleLine"));
    }

    [Test]
    public void SplitLines_ShouldPreserveWhitespace_WhenInputContainsSpaces()
    {
        // Arrange
        var input = "  Line1  \n  Line2  ";

        // Act
        var lines = PurParsingHelpers.SplitLines(input);

        // Assert
        Assert.That(lines[0], Is.EqualTo("  Line1  "));
        Assert.That(lines[1], Is.EqualTo("  Line2  "));
    }
}
