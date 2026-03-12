using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Frends.OracleFusion.SubmitEssJob.Definitions;

namespace Frends.OracleFusion.SubmitEssJob.Helpers;

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
    /// Initializes a new instance of the <see cref="EssJobClient"/> class.
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
    /// Disposes the HTTP client.
    /// </summary>
    public void Dispose()
    {
        httpClient?.Dispose();
    }
}
