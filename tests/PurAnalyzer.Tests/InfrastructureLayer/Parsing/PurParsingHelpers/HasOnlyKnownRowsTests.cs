using PurAnalyzer.Infrastructure.Parsing;

namespace PurAnalyzer.Tests.InfrastructureLayer.Parsing;

[TestFixture]
public class HasOnlyKnownRowsTests
{
    [Test]
    public void HasOnlyKnownRows_ShouldReturnTrue_WhenOnlyH_B_CWithComma()
    {
        var lines = new[]
        {
            "H,one",
            "B,two",
            "C,three"
        };

        var ok = PurParsingHelpers.HasOnlyKnownRows(lines);

        Assert.That(ok, Is.True);
    }

    [Test]
    public void HasOnlyKnownRows_ShouldReturnFalse_WhenUnknownRowTypePresent()
    {
        var lines = new[]
        {
            "H,one",
            "D,unknown", // not allowed
            "B,two"
        };

        var ok = PurParsingHelpers.HasOnlyKnownRows(lines);

        Assert.That(ok, Is.False);
    }
}
