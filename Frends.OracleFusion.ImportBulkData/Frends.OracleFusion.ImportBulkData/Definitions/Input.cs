using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.OracleFusion.ImportBulkData.Definitions;

/// <summary>
/// Essential parameters for FBDI import.
/// </summary>
public class Input
{
    /// <summary>
    /// Array of files to be included in the import ZIP.
    /// </summary>
    /// <example>[{ FileName: "data.csv", Content: "..." }]</example>
    [DefaultValue(null)]
    public File[] Files { get; set; }

    /// <summary>
    /// Name of the ZIP file to upload.
    /// </summary>
    /// <example>MyFiles.zip</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// UCM document account for the upload.
    /// </summary>
    /// <example>fin/cashManagement/import</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string DocumentAccount { get; set; } = string.Empty;
}