using System;

namespace Frends.OracleFusion.ImportBulkData.Definitions;

/// <summary>
/// Error that occurred during the task.
/// </summary>
public class Error
{
    /// <summary>
    /// Summary of the error.
    /// </summary>
    /// <example>Unable to upload file.</example>
    public string Message { get; set; }

    /// <summary>
    /// Additional information about the error.
    /// </summary>
    /// <example>System.IO.IOException: The file is in use by another process.</example>
    public Exception AdditionalInfo { get; set; }
}
