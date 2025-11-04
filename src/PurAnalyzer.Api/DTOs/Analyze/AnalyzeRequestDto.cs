namespace PurAnalyzer.Api.DTOs.Analyze;

/// <summary>
/// Represents the multipart/form-data upload for .PUR analysis.
/// </summary>
public sealed class AnalyzeRequestDto
{
    /// <summary>
    /// Represents an upload request for analyzing a .PUR file.
    /// </summary>
    public IFormFile File { get; set; } = default!;
}
