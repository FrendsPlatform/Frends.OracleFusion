namespace Frends.OracleFusion.ImportBulkData.Helpers;

/// <summary>
/// Request for uploading file to UCM via ERP Integrations API.
/// </summary>
internal class UploadFileToUcmRequest
{
    /// <summary>
    /// Gets or sets the operation name.
    /// </summary>
    /// <example>uploadFileToUCM</example>
    public string OperationName { get; set; }

    /// <summary>
    /// Gets or sets the base64 encoded ZIP file content.
    /// </summary>
    /// <example>UEsDBBQACAgIADdT4VAAAAAA...</example>
    public string DocumentContent { get; set; }

    /// <summary>
    /// Gets or sets the document account.
    /// </summary>
    /// <example>fin/cashManagement/import</example>
    public string DocumentAccount { get; set; }

    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    /// <example>zip</example>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    /// <example>MyFile.zip</example>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the document ID (null for new uploads).
    /// </summary>
    /// <example>null</example>
    public string DocumentId { get; set; }
}
