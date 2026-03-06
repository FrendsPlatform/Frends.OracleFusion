using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Frends.OracleFusion.ImportBulkData.Definitions;
using Frends.OracleFusion.ImportBulkData.Helpers;

namespace Frends.OracleFusion.ImportBulkData;

/// <summary>
/// Task Class for OracleFusion operations.
/// </summary>
public static class OracleFusion
{
    /// <summary>
    /// Uploads FBDI data files to Oracle Fusion UCM using the uploadFileToUCM operation.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-OracleFusion-ImportBulkData)
    /// </summary>
    /// <param name="input">Essential parameters including files and UCM account.</param>
    /// <param name="connection">Connection parameters for Oracle Fusion authentication.</param>
    /// <param name="options">Additional parameters for error handling.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string DocumentId, string Output, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result ImportBulkData(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            return ImportBulkDataAsync(input, connection, options, cancellationToken).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }

    private static async Task<Result> ImportBulkDataAsync(
        Input input,
        Connection connection,
        Options options,
        CancellationToken cancellationToken)
    {
        // Validate required connection parameters
        if (string.IsNullOrWhiteSpace(connection.BaseUrl))
            throw new ArgumentException("BaseUrl is required.", nameof(connection));

        if (string.IsNullOrWhiteSpace(connection.Username))
            throw new ArgumentException("Username is required.", nameof(connection));

        if (string.IsNullOrWhiteSpace(connection.Password))
            throw new ArgumentException("Password is required.", nameof(connection));

        // Validate required input parameters
        if (input.Files == null || input.Files.Length == 0)
            throw new ArgumentException("Files array must contain at least one file.", nameof(input));

        if (string.IsNullOrWhiteSpace(input.FileName))
            throw new ArgumentException("FileName is required.", nameof(input));

        if (string.IsNullOrWhiteSpace(input.DocumentAccount))
            throw new ArgumentException("DocumentAccount is required.", nameof(input));

        // Validate each file
        foreach (var file in input.Files)
        {
            if (string.IsNullOrWhiteSpace(file.FileName))
                throw new ArgumentException("All files must have a FileName.", nameof(input));

            if (string.IsNullOrWhiteSpace(file.Content))
                throw new ArgumentException($"File '{file.FileName}' must have Content.", nameof(input));
        }

        cancellationToken.ThrowIfCancellationRequested();

        // Step 1: Create ZIP file from all files
        string zipBase64;
        using (var zipStream = new MemoryStream())
        {
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var file in input.Files)
                {
                    var fileBytes = Encoding.UTF8.GetBytes(file.Content);

                    // Create entry in ZIP
                    var entry = archive.CreateEntry(file.FileName, CompressionLevel.Optimal);
                    using (var entryStream = entry.Open())
                    {
                        await entryStream.WriteAsync(fileBytes, 0, fileBytes.Length, cancellationToken);
                    }
                }
            }

            // Convert ZIP to base64
            zipStream.Position = 0;
            var zipBytes = zipStream.ToArray();
            zipBase64 = Convert.ToBase64String(zipBytes);
        }

        using var client = new FbdiClient(connection.BaseUrl, connection.Username, connection.Password, connection.ApiVersion);

        // Step 2: Upload file to UCM
        var documentId = await client.UploadFileToUcmAsync(
            zipBase64,
            input.FileName,
            input.DocumentAccount,
            cancellationToken);

        return new Result
        {
            Success = true,
            DocumentId = documentId,
            Output = $"File uploaded successfully to UCM with document ID: {documentId}",
            Error = null,
        };
    }
}