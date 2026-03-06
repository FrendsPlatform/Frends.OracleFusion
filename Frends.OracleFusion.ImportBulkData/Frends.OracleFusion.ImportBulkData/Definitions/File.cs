using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.OracleFusion.ImportBulkData.Definitions;

/// <summary>
/// File to be included in the import.
/// </summary>
public class File
{
    /// <summary>
    /// Name of the file.
    /// </summary>
    /// <example>data.csv</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Content of the file.
    /// </summary>
    /// <example>Hello, world!</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string Content { get; set; } = string.Empty;
}
