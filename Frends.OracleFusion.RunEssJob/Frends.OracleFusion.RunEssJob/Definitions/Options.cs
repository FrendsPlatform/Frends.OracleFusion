using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.OracleFusion.RunEssJob.Definitions;

/// <summary>
/// Additional parameters for ESS job execution.
/// </summary>
public class Options
{
    /// <summary>
    /// Polling interval in seconds to check job status.
    /// </summary>
    /// <example>10</example>
    [DefaultValue(10)]
    public int PollingIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Maximum time in minutes to wait for job completion.
    /// </summary>
    /// <example>60</example>
    [DefaultValue(60)]
    public int TimeoutMinutes { get; set; } = 60;

    /// <summary>
    /// Include log file in the output.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool IncludeLogFile { get; set; } = true;

    /// <summary>
    /// Include output file in the output.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool IncludeOutputFile { get; set; } = true;

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
