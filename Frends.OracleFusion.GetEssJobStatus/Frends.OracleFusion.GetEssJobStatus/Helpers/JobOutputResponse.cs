using System.Text.Json.Serialization;

namespace Frends.OracleFusion.GetEssJobStatus.Helpers;

/// <summary>
/// Response from job output download.
/// </summary>
internal class JobOutputResponse
{
    /// <summary>
    /// Gets or sets the document content (Base64 encoded).
    /// </summary>
    /// <example>UEsDBBQACAgIADdT4VAAAAAA...</example>
    [JsonPropertyName("DocumentContent")]
    public string DocumentContent { get; set; }

    /// <summary>
    /// Gets or sets the request ID.
    /// </summary>
    /// <example>12345</example>
    [JsonPropertyName("ReqstId")]
    public string RequestId { get; set; }

    /// <summary>
    /// Gets or sets the file type.
    /// </summary>
    /// <example>ALL</example>
    [JsonPropertyName("FileType")]
    public string FileType { get; set; }
}
