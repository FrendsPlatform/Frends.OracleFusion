using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.OracleFusion.GetEssJobStatus.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// The Request ID returned by the SubmitEssJob task.
    /// </summary>
    /// <example>12345678</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string RequestId { get; set; } = string.Empty;
}
