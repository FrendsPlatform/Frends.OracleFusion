using System.Text.Json.Serialization;

namespace Frends.OracleFusion.ImportBulkData.Helpers;

/// <summary>
/// Response from upload file to UCM operation.
/// </summary>
internal class UploadFileToUcmResponse
{
    /// <summary>
    /// Gets or sets the document ID.
    /// </summary>
    /// <example>87558082</example>
    [JsonPropertyName("DocumentId")]
    public string DocumentId { get; set; }
}