namespace PurAnalyzer.Application.Parsing;

/// <summary>
/// Represents the structural composition of a .PUR file,
/// indicating whether it contains header (H), body (B), and comment (C) rows.
/// </summary>
public sealed record FileShape
{
    /// <summary>
    /// Indicates if the file contains any header rows (H).
    /// </summary>
    public bool HasH { get; init; }

    /// <summary>
    /// Indicates if the file contains any body rows (B).
    /// </summary>
    public bool HasB { get; init; }

    /// <summary>
    /// Indicates if the file contains any comment rows (C).
    /// </summary>
    public bool HasC { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="FileShape"/> with optional flags.
    /// </summary>
    public FileShape(bool hasH = false, bool hasB = false, bool hasC = false)
    {
        HasH = hasH;
        HasB = hasB;
        HasC = hasC;
    }

    /// <summary>
    /// Parameterless constructor for initialization via object initializer.
    /// </summary>
    public FileShape()
    {
    }
}
