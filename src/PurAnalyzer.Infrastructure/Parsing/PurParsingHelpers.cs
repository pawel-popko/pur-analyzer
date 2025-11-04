using System.Globalization;
using System.Text;
using PurAnalyzer.Application.Parsing;

namespace PurAnalyzer.Infrastructure.Parsing;

/// <summary>
/// Low-level helpers used by the .PUR parser:
/// line splitting, delimiter detection, tolerant number/date parsing, and robust encoding choice.
/// </summary>
/// <remarks>
/// Encoding strategy:
/// 1) UTF-8 with BOM  → treat as UTF-8
/// 2) Windows-1250 (CP-1250) + glyph fix map
/// 3) Valid UTF-8 (no BOM)   → treat as UTF-8
/// 4) ISO-8859-2 (Latin-2)   + glyph fix map
/// </remarks>
public static class PurParsingHelpers
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
    private static readonly UTF8Encoding Utf8Strict = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
    private static readonly UTF8Encoding Utf8Lenient = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);

    // ─────────────────────────────────────────────────────────────────────────────
    // Public helpers
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Normalizes newlines to '\n' and splits into non-empty lines.
    /// </summary>
    public static List<string> SplitLines(string content)
    {
        if (string.IsNullOrEmpty(content)) return new List<string>();
        return content
            .Replace("\r\n", "\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .ToList();
    }

    /// <summary>
    /// Heuristic delimiter detection for a single line. Prefers the character with higher count.
    /// Supports ',' and ';'.
    /// </summary>
    public static char DetectSeparator(string line)
    {
        var semi = Count(line, ';');
        var comma = Count(line, ',');
        if (semi > 0 && comma == 0) return ';';
        if (comma > 0 && semi == 0) return ',';
        return semi >= comma ? ';' : ',';
    }

    public static bool IsHeader(string[] parts) => parts.Length > 0 && parts[0] == "H";
    public static bool IsBody(string[] parts) => parts.Length > 0 && parts[0] == "B";
    public static bool IsComment(string[] p) => p.Length > 0 && p[0] == "C";

    /// <summary>
    /// Tolerant date normalization to ISO format <c>yyyy-MM-dd</c>.
    /// Returns the original string if parsing fails.
    /// </summary>
    public static string NormalizeDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

        if (DateTime.TryParse(raw, Invariant, DateTimeStyles.None, out var dt))
            return dt.ToString("yyyy-MM-dd");

        var formats = new[] { "dd-MM-yyyy", "yyyy-MM-dd", "dd.MM.yyyy" };
        if (DateTime.TryParseExact(raw, formats, Invariant, DateTimeStyles.None, out dt))
            return dt.ToString("yyyy-MM-dd");

        return raw; // fallback
    }

    /// <summary>
    /// Tolerant decimal parsing with invariant culture; returns 0 when missing/invalid.
    /// </summary>
    public static decimal ParseDec(string? s)
        => decimal.TryParse(s, NumberStyles.Any, Invariant, out var d) ? d : 0m;

    /// <summary>
    /// Reads the entire stream as text using a robust encoding strategy (see remarks).
    /// </summary>
    public static async Task<string> ReadAllTextAsync(Stream stream)
    {
        var bytes = await ReadAllBytesAsync(stream);

        // 1) UTF-8 with BOM → treat as UTF-8 (skip BOM)
        if (HasUtf8Bom(bytes))
            return Utf8Lenient.GetString(bytes, 3, bytes.Length - 3);

        // 2) Prefer CP-1250 (with glyph fixes)
        var cp1250 = Encoding.GetEncoding(1250);
        var textCp = cp1250.GetString(bytes);
        var fixedCp = FixPolishArtifacts(textCp);

        // If CP-1250 looks plausible (e.g., contains Polish letters) — return it
        if (ScorePolish(fixedCp) >= 1)
            return fixedCp;

        // 3) Strict UTF-8 — when truly valid
        if (IsValidUtf8(bytes, out var utf8Text))
            return utf8Text!;

        // 4) ISO-8859-2 (with glyph fixes)
        var iso = Encoding.GetEncoding("ISO-8859-2");
        var textIso = iso.GetString(bytes);
        return FixPolishArtifacts(textIso);
    }

    /// <summary>
    /// Computes the file shape (presence of H/B/C rows) based on line prefixes.
    /// </summary>
    /// <remarks>
    /// Assumes rows use a comma separator (e.g., <c>H,</c>, <c>B,</c>, <c>C,</c>).
    /// If semicolons are used, normalize separators before calling this method.
    /// </remarks>
    public static FileShape ComputeFileShape(IReadOnlyList<string> lines)
    {
        bool hasH = false, hasB = false, hasC = false;

        foreach (var l in lines)
        {
            if (string.IsNullOrWhiteSpace(l)) continue;
            if (l.StartsWith("H,")) { hasH = true; continue; }
            if (l.StartsWith("B,")) { hasB = true; continue; }
            if (l.StartsWith("C,")) { hasC = true; continue; }
        }
        return new FileShape(hasH, hasB, hasC);
    }

    /// <summary>
    /// Returns <see langword="true"/> if all non-empty lines start with known record types: H, B or C.
    /// </summary>
    /// <remarks>
    /// Assumes rows use a comma separator (e.g., <c>H,</c>, <c>B,</c>, <c>C,</c>).
    /// If semicolons are used, normalize separators before calling this method.
    /// </remarks>
    public static bool HasOnlyKnownRows(IReadOnlyList<string> lines)
    {
        foreach (var l in lines)
        {
            if (string.IsNullOrWhiteSpace(l)) continue;
            if (!(l.StartsWith("H,") || l.StartsWith("B,") || l.StartsWith("C,")))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Replaces common mis-encoded Polish diacritics with proper characters.
    /// </summary>
    public static string FixPolishArtifacts(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var sb = new StringBuilder(input.Length);
        foreach (var ch in input)
            sb.Append(PolishFixMap.TryGetValue(ch, out var repl) ? repl : ch);
        return sb.ToString();
    }

    /// <summary>
    /// Counts occurrences of a character in a string.
    /// </summary>
    public static int Count(string s, char ch)
    {
        var c = 0;
        for (int i = 0; i < s.Length; i++)
            if (s[i] == ch) c++;
        return c;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Private helpers (encoding)
    // ─────────────────────────────────────────────────────────────────────────────

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream)
    {
        if (stream is MemoryStream ms && ms.CanSeek)
            return ms.ToArray();

        using var mem = new MemoryStream();
        if (stream.CanSeek) stream.Position = 0;
        await stream.CopyToAsync(mem);
        return mem.ToArray();
    }

    private static bool HasUtf8Bom(byte[] bytes)
        => bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;

    /// <summary>
    /// Checks if the byte array is valid UTF-8 by using a strict decoder.
    /// If valid, returns the decoded string in <paramref name="text"/>.
    /// </summary>
    private static bool IsValidUtf8(byte[] bytes, out string? text)
    {
        try
        {
            text = Utf8Strict.GetString(bytes); // throws on invalid UTF-8 sequences

            // Optional round-trip check: encode back and compare
            var roundTripped = Utf8Strict.GetBytes(text);
            if (roundTripped.Length == bytes.Length)
            {
                for (int i = 0; i < bytes.Length; i++)
                    if (bytes[i] != roundTripped[i]) { text = null; return false; }
            }
            return true;
        }
        catch (DecoderFallbackException)
        {
            text = null;
            return false;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Polish artifacts fixer + small scoring
    // ─────────────────────────────────────────────────────────────────────────────

    private static readonly IReadOnlyDictionary<char, char> PolishFixMap = new Dictionary<char, char>
    {
        ['˝'] = 'Ż',
        ['ť'] = 'Ł',
        ['Ť'] = 'Ł',
        ['ŕ'] = 'Ó',
        ['Ă'] = 'Ą',
        ['Â'] = 'Ć',
        ['Ş'] = 'Ś',
        ['Ţ'] = 'Ź',
        ['ă'] = 'Ń',
        ['Ľ'] = 'Ł',
        ['Ĺ'] = 'Ł'
    };

    private static int ScorePolish(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;
        const string pl = "ąćęłńóśżźĄĆĘŁŃÓŚŻŹ";
        int score = 0;
        for (int i = 0; i < s.Length; i++)
            if (pl.IndexOf(s[i]) >= 0) score++;
        return score;
    }
}
