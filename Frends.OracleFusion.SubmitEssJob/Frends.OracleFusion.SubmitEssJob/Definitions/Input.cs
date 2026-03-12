using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.OracleFusion.SubmitEssJob.Definitions;

/// <summary>
/// Essential parameters for ESS job execution.
/// </summary>
public class Input
{
    /// <summary>
    /// The job package name (path to the ESS job).
    /// </summary>
    /// <example>/oracle/apps/ess/financials/payables/invoices/transactions/</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string JobPackageName { get; set; } = string.Empty;

    /// <summary>
    /// The job definition name.
    /// </summary>
    /// <example>APXIAWRE</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string JobDefName { get; set; } = string.Empty;

    /// <summary>
    /// ESS job parameters as a comma-separated string. Use #NULL for null values.
    /// </summary>
    /// <example>12345,#NULL,#NULL,#NULL,678,#NULL,#NULL</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ESSParameters { get; set; } = string.Empty;
}
