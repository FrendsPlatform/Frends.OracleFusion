using System.Text.Json.Serialization;

namespace Frends.OracleFusion.GetEssJobStatus.Helpers;

/// <summary>
/// Response from job status query via ERP Integrations API.
/// </summary>
internal class JobStatusResponse
{
    /// <summary>
    /// Gets or sets the request ID.
    /// </summary>
    /// <example>12345</example>
    [JsonPropertyName("ReqstId")]
    public string RequestId { get; set; }

    /// <summary>
    /// Gets or sets the request status.
    /// </summary>
    /// <example>SUCCEEDED</example>
    [JsonPropertyName("RequestStatus")]
    public string RequestStatus { get; set; }
}
