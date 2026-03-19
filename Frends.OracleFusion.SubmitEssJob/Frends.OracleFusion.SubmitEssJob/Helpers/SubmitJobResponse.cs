using System.Text.Json.Serialization;

namespace Frends.OracleFusion.SubmitEssJob.Helpers;

/// <summary>
/// Response from job submission via ERP Integrations API.
/// </summary>
internal class SubmitJobResponse
{
    /// <summary>
    /// Gets or sets the request ID.
    /// </summary>
    /// <example>12345</example>
    [JsonPropertyName("ReqstId")]
    public string ReqstId { get; set; }
}
