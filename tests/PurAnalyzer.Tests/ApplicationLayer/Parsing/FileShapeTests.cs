using PurAnalyzer.Application.Parsing;

namespace PurAnalyzer.Tests.ApplicationLayer.Parsing
{
    [TestFixture]
    public class FileShapeTests
    {
        [Test]
        public void FileShape_DefaultCtor_SetsAllFlagsFalse()
        {
            // Act
            var shape = new FileShape();

            // Assert
            Assert.That(shape.HasH, Is.False);
            Assert.That(shape.HasB, Is.False);
            Assert.That(shape.HasC, Is.False);
        }

        [Test]
        public void FileShape_ObjectInitializer_AppliesFlagsCorrectly()
        {
            // Act
            var shape = new FileShape { HasH = true, HasB = false, HasC = true };

            // Assert
            Assert.That(shape.HasH, Is.True);
            Assert.That(shape.HasB, Is.False);
            Assert.That(shape.HasC, Is.True);
        }

        [Test]
        public void FileShape_PositionalCtor_AssignsFlagsCorrectly()
        {
            // Act
            var shape = new FileShape(hasH: true, hasB: true, hasC: false);

            // Assert
            Assert.That(shape.HasH, Is.True);
            Assert.That(shape.HasB, Is.True);
            Assert.That(shape.HasC, Is.False);
        }

        [Test]
        public void FileShape_DefaultCtorAndInitializer_AreEqualToPositionalCtor()
        {
            // Arrange
            var viaInitializer = new FileShape { HasH = true, HasB = false, HasC = true };
            var viaPositional = new FileShape(true, false, true);

            // Assert
            Assert.That(viaInitializer, Is.EqualTo(viaPositional));
        }

        [Test]
        public void FileShape_WithExpression_CreatesModifiedCopy()
        {
            // Arrange
            var original = new FileShape { HasH = true, HasB = false, HasC = false };

            // Act
            var modified = original with { HasC = true };

            // Assert
            Assert.That(modified.HasH, Is.True);
            Assert.That(modified.HasB, Is.False);
            Assert.That(modified.HasC, Is.True);
        }
    }
}
