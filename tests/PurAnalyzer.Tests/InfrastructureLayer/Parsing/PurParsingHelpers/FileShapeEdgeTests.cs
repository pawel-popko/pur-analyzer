using PurAnalyzer.Infrastructure.Parsing;

namespace PurAnalyzer.Tests.InfrastructureLayer.Parsing;

[TestFixture]
public class FileShapeEdgeTests
{
    [Test]
    public void ComputeFileShape_ShouldIgnoreWhitespaceOnlyLines()
    {
        var lines = new[] { "   ", "\t", "H,one", "B,two" };
        var shape = PurParsingHelpers.ComputeFileShape(lines);

        Assert.Multiple(() =>
        {
            Assert.That(shape.HasH, Is.True);
            Assert.That(shape.HasB, Is.True);
            Assert.That(shape.HasC, Is.False);
        });
    }

    [Test]
    public void ComputeFileShape_ShouldBeCaseSensitive_AndNotMatchLowercase()
    {
        var lines = new[] { "h,one", "b,two", "c,comment" };
        var shape = PurParsingHelpers.ComputeFileShape(lines);

        Assert.Multiple(() =>
        {
            Assert.That(shape.HasH, Is.False);
            Assert.That(shape.HasB, Is.False);
            Assert.That(shape.HasC, Is.False);
        });
    }

    [Test]
    public void HasOnlyKnownRows_ShouldReturnFalse_WhenSemicolonUsed()
    {
        var lines = new[] { "H;one", "B;two", "C;comment" }; // wrong sep; method expects commas
        var ok = PurParsingHelpers.HasOnlyKnownRows(lines);
        Assert.That(ok, Is.False);
    }

    [Test]
    public void HasOnlyKnownRows_ShouldIgnoreEmptyLines()
    {
        var lines = new[] { "", "   ", "H,one", "B,two" };
        Assert.That(PurParsingHelpers.HasOnlyKnownRows(lines), Is.True);
    }
}
