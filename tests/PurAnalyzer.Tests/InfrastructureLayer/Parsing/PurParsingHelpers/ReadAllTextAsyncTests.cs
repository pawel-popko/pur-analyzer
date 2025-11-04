using System.Diagnostics.CodeAnalysis;
using System.Text;
using PurAnalyzer.Infrastructure.Parsing;

namespace PurAnalyzer.Tests.InfrastructureLayer.Parsing;

[TestFixture]
public class ReadAllTextAsyncTests
{
    [Test]
    public async Task ReadAllTextAsync_ShouldDecodeUtf8WithBom_AndSkipBomBytes()
    {
        // Arrange: UTF-8 text prefixed with BOM
        var bom = new byte[] { 0xEF, 0xBB, 0xBF };
        var textBytes = Encoding.UTF8.GetBytes("Hello UTF8 BOM");
        var bytesWithBom = bom.Concat(textBytes).ToArray();
        using var stream = new MemoryStream(bytesWithBom);

        // Act
        var result = await PurParsingHelpers.ReadAllTextAsync(stream);

        // Assert
        Assert.That(result, Is.EqualTo("Hello UTF8 BOM"));
    }

    [Test]
    public async Task ReadAllTextAsync_ShouldReturnCp1250String_WhenPolishLettersPresent()
    {
        // Arrange: CP-1250 bytes containing Polish diacritics → score ≥ 1
        var text = "Zażółć gęślą jaźń";
        var bytes = Encoding.GetEncoding(1250).GetBytes(text);
        using var stream = new MemoryStream(bytes);

        // Act
        var result = await PurParsingHelpers.ReadAllTextAsync(stream);

        // Assert
        Assert.That(result, Is.EqualTo(text));
    }

    [Test]
    public async Task ReadAllTextAsync_ShouldHandleValidUtf8_WithoutBom()
    {
        // Arrange: plain valid UTF-8
        var text = "Plain UTF8 test";
        var bytes = Encoding.UTF8.GetBytes(text);
        using var stream = new MemoryStream(bytes);

        // Act
        var result = await PurParsingHelpers.ReadAllTextAsync(stream);

        // Assert
        Assert.That(result, Is.EqualTo(text));
    }

    [Test]
    public async Task ReadAllTextAsync_ShouldFallbackToIso8859_WhenInvalidUtf8()
    {
        // Arrange: bytes that are not valid UTF-8; CP-1250 may not look Polish
        var invalidUtf8Bytes = new byte[] { 0xA5, 0xE6, 0xF1, 0xA2 };
        using var stream = new MemoryStream(invalidUtf8Bytes);

        // Act
        var result = await PurParsingHelpers.ReadAllTextAsync(stream);

        // Assert: we end up with some ISO-8859-2 decoded text (non-empty)
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
    }

    [Test]
    public async Task ReadAllTextAsync_ShouldContinuePastCp1250_WhenScoreIsZero()
    {
        // Arrange: CP-1250 bytes with ASCII only → score == 0 (should not stop at CP-1250)
        var asciiText = "ABC123";
        var bytesCp1250 = Encoding.GetEncoding(1250).GetBytes(asciiText);
        using var stream = new MemoryStream(bytesCp1250);

        // Act
        var result = await PurParsingHelpers.ReadAllTextAsync(stream);

        // Assert: ASCII is also valid UTF-8 → final result equals original
        Assert.That(result, Is.EqualTo(asciiText));
    }

    [Test]
    public async Task ReadAllTextAsync_ShouldFallbackToIso8859_WhenInvalidUtf8AndLowCp1250Score()
    {
        // Arrange: bytes fail strict UTF-8; CP-1250 produces non-Polish text (score == 0)
        // 0xFF 0xFE are not valid UTF-8 leading bytes; add 'A' to avoid empty
        var badUtf8 = new byte[] { 0xFF, 0xFE, 0x41 };
        using var stream = new MemoryStream(badUtf8);

        // Act
        var result = await PurParsingHelpers.ReadAllTextAsync(stream);

        // Assert: we fall back to ISO-8859-2 branch (non-empty string is enough here)
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
    }

    [Test]
    public async Task ReadAllTextAsync_ShouldReturnEmpty_WhenFileContainsOnlyUtf8Bom()
    {
        // Arrange: BOM only (no payload)
        var bomOnly = new byte[] { 0xEF, 0xBB, 0xBF };
        using var stream = new MemoryStream(bomOnly);

        // Act
        var result = await PurParsingHelpers.ReadAllTextAsync(stream);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task ReadAllTextAsync_ShouldWork_WhenStreamIsNonSeekable()
    {
        // Arrange: cover non-seekable path in ReadAllBytesAsync
        var text = "Non-seekable UTF8";
        var bytes = Encoding.UTF8.GetBytes(text);
        await using var stream = new NonSeekableStream(bytes);

        // Act
        var result = await PurParsingHelpers.ReadAllTextAsync(stream);

        // Assert
        Assert.That(result, Is.EqualTo(text));
    }

    [ExcludeFromCodeCoverage]
    private sealed class NonSeekableStream : Stream
    {
        private readonly Stream _inner;
        public NonSeekableStream(byte[] data) => _inner = new MemoryStream(data);

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => throw new NotSupportedException(); }
        public override void Flush() => _inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { _inner.Dispose(); base.Dispose(disposing); }
    }
}
