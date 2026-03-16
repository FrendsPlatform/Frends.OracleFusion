using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Frends.OracleFusion.GetEssJobStatus.Helpers;

/// <summary>
/// Wrapper for job status response.
/// </summary>
internal class JobStatusResponseWrapper
{
    /// <summary>
    /// Gets or sets the items.
    /// </summary>
    /// <example>[]</example>
    [JsonPropertyName("items")]
    public List<JobStatusResponse> Items { get; set; }
}
