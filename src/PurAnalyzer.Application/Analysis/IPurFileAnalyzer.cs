using PurAnalyzer.Application.Parsing;

namespace PurAnalyzer.Application.Analysis;

/// <summary>
/// Defines the contract for analyzing parsed .PUR file data
/// and computing resulting metrics and summaries.
/// </summary>
public interface IPurFileAnalyzer
{
    /// <summary>
    /// Performs analysis of parsed .PUR documents and computes aggregate metrics.
    /// </summary>
    /// <param name="documents">List of parsed documents to analyze.</param>
    /// <param name="lineCount">Total number of lines in the original file.</param>
    /// <param name="charCount">Total number of characters in the original file.</param>
    /// <param name="x">
    /// Threshold used for <c>XCount</c> calculation —
    /// number of documents with more item positions than this value.
    /// </param>
    /// <param name="shape">File shape summary indicating presence of H/B/C rows.</param>
    /// <returns>Result of the analysis containing computed statistics and message details.</returns>
    AnalysisResult Analyze(
        IReadOnlyList<Document> documents,
        int lineCount,
        int charCount,
        int x,
        FileShape shape);
}