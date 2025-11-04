using PurAnalyzer.Infrastructure.Parsing;

namespace PurAnalyzer.Tests.InfrastructureLayer.Parsing;

[TestFixture]
public class CountTests
{
    [Test]
    public void Count_ShouldReturnNumberOfOccurrences_WhenCharRepeats()
    {
        var s = "A;B;C;;;"; // 5 średników
        Assert.That(PurParsingHelpers.Count(s, ';'), Is.EqualTo(5));
    }

    [Test]
    public void Count_ShouldReturnZero_WhenCharNotPresent()
    {
        Assert.That(PurParsingHelpers.Count("ABC", ';'), Is.EqualTo(0));
    }
}
