using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Frends.OracleFusion.GetEssJobStatus.Helpers;

/// <summary>
/// Wrapper for job output response.
/// </summary>
internal class JobOutputResponseWrapper
{
    /// <summary>
    /// Gets or sets the items.
    /// </summary>
    /// <example>[]</example>
    [JsonPropertyName("items")]
    public List<JobOutputResponse> Items { get; set; }
}
