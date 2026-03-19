using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.OracleFusion.ImportBulkData.Helpers;

/// <summary>
/// HTTP client wrapper for Oracle Fusion FBDI import operations.
/// </summary>
internal class FbdiClient : IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = null, // Keep original casing (PascalCase)
    };

    private readonly HttpClient httpClient;
    private readonly string baseUrl;
    private readonly string apiVersion;

    /// <summary>
    /// Initializes a new instance of the <see cref="FbdiClient"/> class.
    /// </summary>
    /// <param name="baseUrl">Oracle Fusion base URL.</param>
    /// <param name="username">Username for basic authentication.</param>
    /// <param name="password">Password for basic authentication.</param>
    /// <param name="apiVersion">API version for the fscmRestApi endpoint.</param>
    /// <param name="timeout">Timeout in seconds for HTTP requests to the Oracle Fusion API.</param>
    public FbdiClient(string baseUrl, string username, string password, string apiVersion, int timeout)
        : this(baseUrl, username, password, apiVersion, timeout, new HttpClient())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FbdiClient"/> class with an injected HTTP client.
    /// </summary>
    /// <param name="baseUrl">Oracle Fusion base URL.</param>
    /// <param name="username">Username for basic authentication.</param>
    /// <param name="password">Password for basic authentication.</param>
    /// <param name="apiVersion">API version for the fscmRestApi endpoint.</param>
    /// <param name="timeout">Timeout in seconds for HTTP requests to the Oracle Fusion API.</param>
    /// <param name="httpClient">HTTP client instance. Used for injecting mocked clients in tests.</param>
    internal FbdiClient(string baseUrl, string username, string password, string apiVersion, int timeout, HttpClient httpClient)
    {
        this.baseUrl = baseUrl.TrimEnd('/');
        this.apiVersion = apiVersion;
        this.httpClient = httpClient;
        httpClient.Timeout = TimeSpan.FromSeconds(timeout);
        var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
    }

    /// <summary>
    /// Uploads a file to UCM and returns the document ID.
    /// </summary>
    /// <param name="documentContent">Base64 encoded ZIP file content.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="documentAccount">UCM document account.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>UCM document ID.</returns>
    public async Task<string> UploadFileToUcmAsync(
        string documentContent,
        string fileName,
        string documentAccount,
        CancellationToken cancellationToken)
    {
        var requestBody = new UploadFileToUcmRequest
        {
            OperationName = "uploadFileToUCM",
            DocumentContent = documentContent,
            ContentType = "zip",
            FileName = fileName,
            DocumentAccount = documentAccount,
            DocumentId = null,
        };

        var url = $"{baseUrl}/fscmRestApi/resources/{apiVersion}/erpintegrations";

        var json = JsonSerializer.Serialize(requestBody, SerializerOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await httpClient.PostAsync(url, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Failed to upload file to UCM. Status: {response.StatusCode}, Error: {errorContent}");
        }

        var result = await response.Content.ReadFromJsonAsync<UploadFileToUcmResponse>(cancellationToken);
        return result?.DocumentId ?? throw new Exception("Failed to parse UCM upload response - no document ID returned.");
    }

    /// <summary>
    /// Disposes the HTTP client.
    /// </summary>
    public void Dispose()
    {
        httpClient?.Dispose();
    }
}