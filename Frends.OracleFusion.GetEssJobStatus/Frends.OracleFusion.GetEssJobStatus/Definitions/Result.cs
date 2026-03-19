using System;
using System.Collections.Generic;
using Frends.OracleFusion.GetEssJobStatus.Helpers;

namespace Frends.OracleFusion.GetEssJobStatus.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates if the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Indicates whether the job has reached a terminal state (SUCCEEDED, FAILED, WARNING, CANCELLED, ERROR).
    /// When false, the job is still running or queued. Use RequestId to check again later.
    /// </summary>
    /// <example>true</example>
    public bool IsCompleted { get; set; }

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
    /// The UTC timestamp of when this status check was performed.
    /// </summary>
    /// <example>2024-06-30T10:24:07.723Z</example>
    public DateTime? StatusCheckedAt { get; set; }

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
    /// List of files extracted from the ESS job output ZIP archive. Populated when IncludeLogFile or IncludeOutputFile is enabled and the job has completed.
    /// </summary>
    /// <example>[{ "FileName": "report.csv", "Content": "col1,col2\nval1,val2" }]</example>
    public List<ExtractedFile> OutputFiles { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
