namespace PurAnalyzer.Application.Parsing;

/// <summary>
/// Defines a contract for parsing .PUR files into normalized document data.
/// Implementations live in the Infrastructure layer.
/// </summary>
public interface IPurFileParser
{
    /// <summary>
    /// Reads a .PUR file stream and extracts structured content.
    /// </summary>
    Task<ParsedContent> ParseAsync(Stream stream);
}
