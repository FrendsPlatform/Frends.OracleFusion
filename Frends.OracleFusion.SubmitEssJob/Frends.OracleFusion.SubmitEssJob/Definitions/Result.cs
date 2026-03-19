using System;

namespace Frends.OracleFusion.SubmitEssJob.Definitions;

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
    /// <example>1234</example>
    public string RequestId { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
