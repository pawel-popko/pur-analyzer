using Microsoft.AspNetCore.Mvc;

namespace PurAnalyzer.Api.Validation;

/// <summary>
/// Validates incoming HTTP requests for the .PUR analysis endpoint.
/// Keeps controller thin and testable.
/// </summary>
public sealed class PurFileValidator
{
    /// <summary>
    /// Maximum allowed file size in bytes (10 MB).
    /// </summary>
    public const long MaxFileBytes = 10L * 1024 * 1024; // 10 MB

    /// <summary>
    /// Validates route parameter 'x'. Returns ProblemDetails on error; null on success.
    /// </summary>
    public ProblemDetails? ValidateRouteX(int x)
    {
        if (x < 0)
        {
            return new ProblemDetails
            {
                Title = "Invalid request",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Route parameter 'x' must be greater than or equal to 0."
            };
        }
        return null;
    }

    /// <summary>
    /// Validates that the request is multipart/form-data with exactly one .PUR file under 10 MB.
    /// Returns (file, problem) where problem!=null indicates an error to be returned.
    /// </summary>
    public async Task<(IFormFile? File, ProblemDetails? Problem)> ValidateSingleFileAsync(HttpRequest request)
    {
        if (!request.HasFormContentType)
        {
            return (null, Problem415("Please upload exactly one .PUR file using multipart/form-data."));
        }

        var form = await request.ReadFormAsync();

        if (form.Files.Count != 1)
        {
            return (null, Problem415("Please upload exactly one .PUR file using multipart/form-data."));
        }

        var file = form.Files[0];

        if (!file.FileName.EndsWith(".PUR", StringComparison.OrdinalIgnoreCase))
        {
            return (null, Problem415("Unsupported file extension. Please upload a .PUR file."));
        }

        if (file.Length > MaxFileBytes)
        {
            return (null, new ProblemDetails
            {
                Title = "File too large",
                Status = StatusCodes.Status413PayloadTooLarge,
                Detail = "Uploaded file exceeds the 10 MB limit. Please reduce the file size and try again."
            });
        }

        return (file, null);
    }

    private static ProblemDetails Problem415(string detail) => new()
    {
        Title = "Invalid upload",
        Status = StatusCodes.Status415UnsupportedMediaType,
        Detail = detail
    };
}
