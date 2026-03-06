using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Frends.OracleFusion.ImportBulkData.Definitions;

namespace Frends.OracleFusion.ImportBulkData.Helpers;

/// <summary>
/// HTTP client wrapper for Oracle Fusion FBDI import operations.
/// </summary>
internal class FbdiClient : IDisposable
{
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
    public FbdiClient(string baseUrl, string username, string password, string apiVersion)
    {
        this.baseUrl = baseUrl.TrimEnd('/');
        this.apiVersion = apiVersion;
        httpClient = new HttpClient();

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

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null, // Keep original casing (PascalCase)
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        };

        var json = JsonSerializer.Serialize(requestBody, options);
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

/// <summary>
/// Request for uploading file to UCM via ERP Integrations API.
/// </summary>
internal class UploadFileToUcmRequest
{
    /// <summary>
    /// Gets or sets the operation name.
    /// </summary>
    /// <example>uploadFileToUCM</example>
    public string OperationName { get; set; }

    /// <summary>
    /// Gets or sets the base64 encoded ZIP file content.
    /// </summary>
    /// <example>UEsDBBQACAgIADdT4VAAAAAA...</example>
    public string DocumentContent { get; set; }

    /// <summary>
    /// Gets or sets the document account.
    /// </summary>
    /// <example>fin/cashManagement/import</example>
    public string DocumentAccount { get; set; }

    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    /// <example>zip</example>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    /// <example>MyFile.zip</example>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the document ID (null for new uploads).
    /// </summary>
    /// <example>null</example>
    public string DocumentId { get; set; }
}

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