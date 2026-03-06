using System;

namespace Frends.OracleFusion.RunEssJob.Definitions;

/// <summary>
/// Result of the ESS job execution.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates if the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// ESS job request ID.
    /// </summary>
    /// <example>12345</example>
    public string RequestId { get; set; }

    /// <summary>
    /// Final job status (SUCCEEDED, FAILED, WARNING, CANCELLED, ERROR, etc.).
    /// </summary>
    /// <example>SUCCEEDED</example>
    public string JobStatus { get; set; }

    /// <summary>
    /// Detailed status description.
    /// </summary>
    /// <example>Request execution was successful</example>
    public string JobStatusDescription { get; set; }

    /// <summary>
    /// Job completion timestamp.
    /// </summary>
    /// <example>2024-06-30T10:24:07.723Z</example>
    public DateTime? CompletedTime { get; set; }

    /// <summary>
    /// Base64 encoded log file content (if requested and available).
    /// </summary>
    /// <example>UEsDBBQACAgIADdT4VAAAAAA...</example>
    public string LogFileContent { get; set; }

    /// <summary>
    /// Base64 encoded output file content (if requested and available).
    /// </summary>
    /// <example>UEsDBBQACAgIADdT4VAAAAAA...</example>
    public string OutputFileContent { get; set; }

    /// <summary>
    /// Decoded text output for convenience.
    /// </summary>
    /// <example>Job completed successfully with 100 records processed.</example>
    public string Output { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
