namespace Frends.OracleFusion.ImportBulkData.Definitions;

/// <summary>
/// Result of the FBDI import task.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates if the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// UCM document ID for the uploaded file.
    /// </summary>
    /// <example>12345678</example>
    public string DocumentId { get; set; }

    /// <summary>
    /// Output message.
    /// </summary>
    /// <example>File uploaded successfully to UCM with document ID: 12345678</example>
    public string Output { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
