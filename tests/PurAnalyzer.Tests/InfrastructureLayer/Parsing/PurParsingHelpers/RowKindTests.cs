using PurAnalyzer.Infrastructure.Parsing;

namespace PurAnalyzer.Tests.InfrastructureLayer.Parsing;

[TestFixture]
public class RowKindTests
{
    [Test]
    public void RowKind_Detectors_ShouldReturnTrue_ForValidPrefixes()
    {
        Assert.That(PurParsingHelpers.IsHeader(new[] { "H" }), Is.True);
        Assert.That(PurParsingHelpers.IsBody(new[] { "B" }), Is.True);
        Assert.That(PurParsingHelpers.IsComment(new[] { "C" }), Is.True);
    }

    [Test]
    public void RowKind_Detectors_ShouldReturnFalse_ForEmptyOrOther()
    {
        Assert.That(PurParsingHelpers.IsHeader(Array.Empty<string>()), Is.False);
        Assert.That(PurParsingHelpers.IsBody(new[] { "X" }), Is.False);
        Assert.That(PurParsingHelpers.IsComment(new[] { "" }), Is.False);
    }
}
