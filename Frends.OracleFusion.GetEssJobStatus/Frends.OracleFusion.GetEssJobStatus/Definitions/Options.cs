using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.OracleFusion.GetEssJobStatus.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// When true, polls until the job reaches a terminal state or timeout is reached.
    /// When false, checks the job status once and returns immediately.
    /// </summary>
    /// <example>true</example>
    public bool WaitForCompletion { get; set; } = false;

    /// <summary>
    /// Polling interval in seconds to check job status.
    /// </summary>
    /// <example>10</example>
    [UIHint(nameof(WaitForCompletion), "", true)]
    [DefaultValue(10)]
    public int PollingIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Maximum time in minutes to wait for job completion.
    /// </summary>
    /// <example>60</example>
    [UIHint(nameof(WaitForCompletion), "", true)]
    [DefaultValue(60)]
    public int TimeoutMinutes { get; set; } = 60;

    /// <summary>
    /// Include log file in the output.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool IncludeLogFile { get; set; } = false;

    /// <summary>
    /// Include output file in the output.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool IncludeOutputFile { get; set; } = false;

    /// <summary>
    /// Whether to throw an error on failure.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Custom error message</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
