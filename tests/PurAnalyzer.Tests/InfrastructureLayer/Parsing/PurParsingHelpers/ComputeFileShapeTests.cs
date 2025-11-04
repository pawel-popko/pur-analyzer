using PurAnalyzer.Application.Parsing;
using PurAnalyzer.Infrastructure.Parsing;

namespace PurAnalyzer.Tests.InfrastructureLayer.Parsing;

[TestFixture]
public class ComputeFileShapeTests
{
    [Test]
    public void ComputeFileShape_ShouldDetectPresenceOfH_B_C_WhenCommaSeparated()
    {
        // Arrange (comma-separated as required by the helper)
        var lines = new[]
        {
            "H,field1,field2",
            "B,field1,field2",
            "C,comment"
        };

        // Act
        FileShape shape = PurParsingHelpers.ComputeFileShape(lines);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(shape.HasH, Is.True);
            Assert.That(shape.HasB, Is.True);
            Assert.That(shape.HasC, Is.True);
        });
    }
}
