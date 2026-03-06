using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Frends.OracleFusion.RunEssJob.Definitions;

namespace Frends.OracleFusion.RunEssJob.Helpers;

/// <summary>
/// HTTP client wrapper for Oracle Fusion ERP Integrations API.
/// </summary>
internal class EssJobClient : IDisposable
{
    private readonly HttpClient httpClient;
    private readonly string baseUrl;
    private readonly string apiVersion;

    /// <summary>
    /// Initializes a new instance of the <see cref="EssJobClient"/> class.
    /// </summary>
    /// <param name="baseUrl">Oracle Fusion base URL.</param>
    /// <param name="username">Username for basic authentication.</param>
    /// <param name="password">Password for basic authentication.</param>
    /// <param name="apiVersion">API version for the fscmRestApi endpoint.</param>
    public EssJobClient(string baseUrl, string username, string password, string apiVersion)
    {
        this.baseUrl = baseUrl.TrimEnd('/');
        this.apiVersion = apiVersion;
        httpClient = new HttpClient();

        var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Submits an ESS job and returns the request ID.
    /// </summary>
    /// <param name="input">Input parameters for the job.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Job request ID.</returns>
    public async Task<string> SubmitJobAsync(Input input, CancellationToken cancellationToken)
    {
        var requestBody = new SubmitJobRequest
        {
            OperationName = "submitESSJobRequest",
            JobPackageName = input.JobPackageName,
            JobDefName = input.JobDefName,
            ESSParameters = string.IsNullOrEmpty(input.ESSParameters) ? null : input.ESSParameters,
        };

        var url = $"{baseUrl}/fscmRestApi/resources/{apiVersion}/erpintegrations";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null, // Keep original casing (PascalCase)
        };

        var json = JsonSerializer.Serialize(requestBody, options);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(url, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Failed to submit ESS job. Status: {response.StatusCode}, Error: {errorContent}");
        }

        var result = await response.Content.ReadFromJsonAsync<SubmitJobResponse>(cancellationToken);
        return result?.ReqstId ?? throw new Exception("Failed to parse job submission response - no request ID returned.");
    }

    /// <summary>
    /// Gets the status of an ESS job.
    /// </summary>
    /// <param name="requestId">Job request ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Job status information.</returns>
    public async Task<JobStatusResponse> GetJobStatusAsync(string requestId, CancellationToken cancellationToken)
    {
        var url = $"{baseUrl}/fscmRestApi/resources/{apiVersion}/erpintegrations?finder=ESSJobStatusRF;requestId={requestId}";
        var response = await httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Failed to get job status. Status: {response.StatusCode}, Error: {errorContent}");
        }

        var wrapper = await response.Content.ReadFromJsonAsync<JobStatusResponseWrapper>(cancellationToken);
        var result = wrapper?.Items?.FirstOrDefault();
        return result ?? throw new Exception("Failed to parse job status response.");
    }

    /// <summary>
    /// Downloads ESS job execution details (output and logs).
    /// </summary>
    /// <param name="requestId">Job request ID.</param>
    /// <param name="includeLog">Include log file.</param>
    /// <param name="includeOutput">Include output file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Job output response.</returns>
    public async Task<JobOutputResponse> DownloadJobOutputAsync(
        string requestId,
        bool includeLog,
        bool includeOutput,
        CancellationToken cancellationToken)
    {
        string fileType = "ALL";
        if (includeLog && !includeOutput)
            fileType = "LOG";
        else if (includeOutput && !includeLog)
            fileType = "OUT";

        var url = $"{baseUrl}/fscmRestApi/resources/{apiVersion}/erpintegrations?finder=ESSJobExecutionDetailsRF;requestId={requestId},fileType={fileType}";

        var response = await httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Failed to download job output. Status: {response.StatusCode}, Error: {errorContent}");
        }

        var result = await response.Content.ReadFromJsonAsync<JobOutputResponseWrapper>(cancellationToken);
        return result?.Items?.FirstOrDefault() ?? throw new Exception("Failed to parse job output response.");
    }

    /// <summary>
    /// Disposes the HTTP client.
    /// </summary>
    public void Dispose()
    {
        httpClient?.Dispose();
    }
}

/// <summary>
/// Request for job submission via ERP Integrations API.
/// </summary>
internal class SubmitJobRequest
{
    /// <summary>
    /// Gets or sets the operation name.
    /// </summary>
    /// <example>submitESSJobRequest</example>
    public string OperationName { get; set; }

    /// <summary>
    /// Gets or sets the job package name.
    /// </summary>
    /// <example>/oracle/apps/ess/financials/payables/invoices/transactions/</example>
    public string JobPackageName { get; set; }

    /// <summary>
    /// Gets or sets the job definition name.
    /// </summary>
    /// <example>APXIAWRE</example>
    public string JobDefName { get; set; }

    /// <summary>
    /// Gets or sets the ESS parameters.
    /// </summary>
    /// <example>12345,#NULL,#NULL,#NULL,678,#NULL,#NULL</example>
    public string ESSParameters { get; set; }
}

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
    public string ReqstId { get; set; }

    /// <summary>
    /// Gets or sets the request status.
    /// </summary>
    /// <example>SUCCEEDED</example>
    [JsonPropertyName("RequestStatus")]
    public string RequestStatus { get; set; }

    /// <summary>
    /// Gets the status (same as RequestStatus for compatibility).
    /// </summary>
    /// <example>SUCCEEDED</example>
    public string Status => RequestStatus ?? "UNKNOWN";
}

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
    public string ReqstId { get; set; }

    /// <summary>
    /// Gets or sets the file type.
    /// </summary>
    /// <example>ALL</example>
    [JsonPropertyName("FileType")]
    public string FileType { get; set; }
}
