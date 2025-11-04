using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PurAnalyzer.Api.DTOs.Analyze;
using PurAnalyzer.Api.Validation;
using PurAnalyzer.Application.Analysis;
using PurAnalyzer.Application.Parsing;
using PurAnalyzer.Infrastructure.Parsing;

namespace PurAnalyzer.Api.Controllers.v1;

/// <summary>
/// v1 endpoint for analyzing a single .PUR file.
/// Thin controller: delegates validation, parsing, analysis, and persistence to dedicated services.
/// </summary>
[ApiController]
[Route("api/v1/analyze")]
public sealed class AnalyzeController : ControllerBase
{
    private readonly PurFileValidator _validator;
    private readonly IPurFileParser _parser;
    private readonly IPurFileAnalyzer _analyzer;
    private readonly IDocumentWriter _writer;

    public AnalyzeController(
        PurFileValidator validator,
        IPurFileParser parser,
        IPurFileAnalyzer analyzer,
        IDocumentWriter writer)
    {
        _validator = validator;
        _parser = parser;
        _analyzer = analyzer;
        _writer = writer;
    }

    /// <summary>
    /// Analyzes a single .PUR file and returns parsed documents and metrics.
    /// </summary>
    /// <param name="x">Threshold used for XCount calculation (documents with more items than this value).</param>
    /// <param name="request">Uploaded .PUR file wrapped in a multipart/form-data request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>HTTP 200 with analysis result or ProblemDetails with error description.</returns>
    [HttpPost("{x:int}")]
    [Authorize(AuthenticationSchemes = "Basic")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AnalyzeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Analyze(
        [FromRoute] int x,
        [FromForm] AnalyzeRequestDto request,
        CancellationToken ct)
    {
        var xProblem = _validator.ValidateRouteX(x);
        if (xProblem is not null)
            return ToProblem(xProblem);

        var file = request.File;
        if (file is null)
            return Problem(statusCode: 415, title: "Invalid upload", detail: "Please upload exactly one .PUR file.");

        if (file.Length > PurFileValidator.MaxFileBytes)
            return Problem(statusCode: 413, title: "File too large", detail: "Uploaded file exceeds the 10 MB limit.");

        await using var stream = file.OpenReadStream();
        var parsed = await _parser.ParseAsync(stream);

        // Basic validation: headers without body rows
        if (parsed.FormatIssues.Any(i =>
                i.Contains("follows header", StringComparison.OrdinalIgnoreCase) ||
                i.Contains("has no item rows (B)", StringComparison.OrdinalIgnoreCase)))
        {
            return Problem(
                statusCode: 422,
                title: "Invalid file format",
                detail: "The file contains header rows (H) without corresponding item rows (B).");
        }

        // Check file shape (presence of H/B/C)
        await using var shapeStream = file.OpenReadStream();
        var content = await PurParsingHelpers.ReadAllTextAsync(shapeStream);
        var lines = PurParsingHelpers.SplitLines(content);
        var shape = PurParsingHelpers.ComputeFileShape(lines);

        if (!PurParsingHelpers.HasOnlyKnownRows(lines))
        {
            return Problem(
                statusCode: 422,
                title: "Invalid file format",
                detail: "The file contains unknown row types (expected only H, B, or C).");
        }

        if (!shape.HasH && shape.HasB)
        {
            return Problem(
                statusCode: 422,
                title: "Invalid file format",
                detail: "The file contains item rows (B) without any header row (H).");
        }

        if (shape.HasH && !shape.HasB)
        {
            return Problem(
                statusCode: 422,
                title: "Invalid file format",
                detail: "The file contains header rows (H) but no item rows (B).");
        }

        if (parsed.LineCount == 0)
        {
            return Problem(
                statusCode: 422,
                title: "Unprocessable file format",
                detail: "The uploaded file does not match the expected .PUR structure.");
        }

        // --- Analyze in-memory
        var result = _analyzer.Analyze(parsed.Documents, parsed.LineCount, parsed.CharCount, x, shape);

        // --- Persist into database
        var saveSummary = await _writer.SaveAsync(parsed.Documents, ct);

        // --- Build DTO
        var dto = new AnalyzeResponseDto
        {
            Documents = parsed.Documents.Select(d => new DocumentDto
            {
                BaCode = d.BaCode,
                Type = d.Type,
                DocumentNumber = d.DocumentNumber ?? string.Empty,
                OperationDate = d.OperationDate,
                DocumentDayNumber = d.DocumentDayNumber,
                ContractorCode = d.ContractorCode ?? string.Empty,
                ContractorName = d.ContractorName,
                ExternalDocumentNumber = d.ExternalDocumentNumber,
                ExternalDocumentDate = d.ExternalDocumentDate,
                NetTotal = d.NetTotal,
                VatTotal = d.VatTotal,
                GrossTotal = d.GrossTotal,
                Flag1 = d.Flag1,
                Flag2 = d.Flag2,
                Flag3 = d.Flag3,
                Comment = d.Comment,
                Items = (d.Items ?? Array.Empty<DocumentItem>()).Select(i => new DocumentItemDto
                {
                    ProductCode = i.ProductCode ?? string.Empty,
                    ProductName = i.ProductName ?? string.Empty,
                    Quantity = i.Quantity ?? 0m,
                    UnitPriceNet = i.UnitPriceNet ?? 0m,
                    NetValue = i.NetValue ?? 0m,
                    VatValue = i.VatValue ?? 0m,
                    QuantityBefore = i.QuantityBefore ?? 0m,
                    AverageBefore = i.AverageBefore ?? 0m,
                    QuantityAfter = i.QuantityAfter ?? 0m,
                    AverageAfter = i.AverageAfter ?? 0m,
                    ProductGroup = i.ProductGroup
                }).ToArray()
            }).ToArray(),
            LineCount = result.LineCount,
            CharCount = result.CharCount,
            PositionsCount = result.PositionsCount,
            XCount = result.XCount,
            ProductsWithMaxNetValue = result.ProductsWithMaxNetValue,
            Message = result.Message
        };

        return Ok(dto);
    }

    /// <summary>
    /// Converts a ProblemDetails object into an IActionResult with RFC7807 shape.
    /// </summary>
    private IActionResult ToProblem(ProblemDetails problem)
    {
        return Problem(
            statusCode: problem.Status ?? StatusCodes.Status400BadRequest,
            title: problem.Title ?? "Invalid request",
            detail: problem.Detail);
    }
}
