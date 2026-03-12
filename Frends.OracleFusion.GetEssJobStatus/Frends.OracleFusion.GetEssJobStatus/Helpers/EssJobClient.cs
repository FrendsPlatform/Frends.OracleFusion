using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.OracleFusion.GetEssJobStatus.Helpers;

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
        : this(baseUrl, username, password, apiVersion, new HttpClient())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EssJobClient"/> class with an injected HTTP client.
    /// </summary>
    /// <param name="baseUrl">Oracle Fusion base URL.</param>
    /// <param name="username">Username for basic authentication.</param>
    /// <param name="password">Password for basic authentication.</param>
    /// <param name="apiVersion">API version for the fscmRestApi endpoint.</param>
    /// <param name="httpClient">HTTP client instance. Used for injecting mocked clients in tests.</param>
    internal EssJobClient(string baseUrl, string username, string password, string apiVersion, HttpClient httpClient)
    {
        this.baseUrl = baseUrl.TrimEnd('/');
        this.apiVersion = apiVersion;
        this.httpClient = httpClient;

        var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
        using var response = await httpClient.GetAsync(url, cancellationToken);

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

        using var response = await httpClient.GetAsync(url, cancellationToken);

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
