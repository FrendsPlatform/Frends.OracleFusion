using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Frends.OracleFusion.GetEssJobStatus.Definitions;
using Frends.OracleFusion.GetEssJobStatus.Helpers;

namespace Frends.OracleFusion.GetEssJobStatus;

/// <summary>
/// Task Class for OracleFusion operations.
/// </summary>
public static class OracleFusion
{
    /// <summary>
    /// Factory for creating EssJobClient instances. Can be overridden in tests to inject a mock client.
    /// </summary>
    internal static Func<string, string, string, string, EssJobClient> EssJobClientConstructor { get; set; }
    = (baseUrl, username, password, apiVersion)
        => new EssJobClient(baseUrl, username, password, apiVersion);

    /// <summary>
    /// Frends task to check and optionally wait for completion of an Oracle Fusion ESS job.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-OracleFusion-GetEssJobStatus)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string RequestId, string JobStatus, string JobStatusDescription, DateTime? CompletedTime, string LogFileContent, string OutputFileContent, string Output, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result GetEssJobStatus(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            return GetEssJobStatusAsync(input, connection, options, cancellationToken).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }

    private static async Task<Result> GetEssJobStatusAsync(
        Input input,
        Connection connection,
        Options options,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.RequestId))
            throw new ArgumentException("RequestId is required.", nameof(input));

        if (string.IsNullOrWhiteSpace(connection.BaseUrl))
            throw new ArgumentException("BaseUrl is required.", nameof(connection));

        if (string.IsNullOrWhiteSpace(connection.Username))
            throw new ArgumentException("Username is required.", nameof(connection));

        if (string.IsNullOrWhiteSpace(connection.Password))
            throw new ArgumentException("Password is required.", nameof(connection));

        cancellationToken.ThrowIfCancellationRequested();

        using var client = EssJobClientConstructor(connection.BaseUrl, connection.Username, connection.Password, connection.ApiVersion);

        JobStatusResponse statusResponse = null;
        var isTerminalState = false;

        if (options.WaitForCompletion)
        {
            var timeoutTime = DateTime.UtcNow.AddMinutes(options.TimeoutMinutes);

            while (!isTerminalState && DateTime.UtcNow < timeoutTime)
            {
                cancellationToken.ThrowIfCancellationRequested();
                statusResponse = await client.GetJobStatusAsync(input.RequestId, cancellationToken);
                isTerminalState = IsTerminalState(statusResponse.Status);

                if (!isTerminalState)
                    await Task.Delay(TimeSpan.FromSeconds(options.PollingIntervalSeconds), cancellationToken);
            }

            if (!isTerminalState)
            {
                throw new TimeoutException(
                    $"Job did not complete within {options.TimeoutMinutes} minutes. " +
                    $"Last known status: {statusResponse?.Status}");
            }
        }
        else
        {
            statusResponse = await client.GetJobStatusAsync(input.RequestId, cancellationToken);
            isTerminalState = IsTerminalState(statusResponse.Status);
        }

        string logFileContent = null;
        string outputFileContent = null;
        string decodedOutput = null;

        if ((options.IncludeLogFile || options.IncludeOutputFile) &&
            (statusResponse.Status == "SUCCEEDED"
             || statusResponse.Status == "WARNING"
             || statusResponse.Status == "FAILED"
             || statusResponse.Status == "ERROR"))
        {
            try
            {
                var outputResponse = await client.DownloadJobOutputAsync(
                    input.RequestId,
                    options.IncludeLogFile,
                    options.IncludeOutputFile,
                    cancellationToken);

                if (options.IncludeLogFile && options.IncludeOutputFile)
                {
                    logFileContent = outputResponse.DocumentContent;
                    outputFileContent = outputResponse.DocumentContent;
                }
                else if (options.IncludeLogFile)
                {
                    logFileContent = outputResponse.DocumentContent;
                }
                else if (options.IncludeOutputFile)
                {
                    outputFileContent = outputResponse.DocumentContent;
                }

                if (!string.IsNullOrEmpty(outputResponse.DocumentContent))
                {
                    try
                    {
                        var zipBytes = Convert.FromBase64String(outputResponse.DocumentContent);
                        decodedOutput = ExtractZipContent(zipBytes);
                    }
                    catch (Exception ex)
                    {
                        decodedOutput = $"Failed to extract ZIP content: {ex.Message}. ZIP file size: {outputResponse.DocumentContent.Length} chars (base64).";
                    }
                }
            }
            catch (Exception ex)
            {
                decodedOutput = $"Failed to retrieve job output: {ex.Message}";
            }
        }

        var success = statusResponse.Status == "SUCCEEDED" || statusResponse.Status == "WARNING";

        if (isTerminalState && !success && options.ThrowErrorOnFailure)
        {
        throw new Exception(
            $"ESS job failed. Status: {statusResponse.Status}, Request ID: {statusResponse.ReqstId}");
        }

        return new Result
        {
            Success = success,
            IsCompleted = isTerminalState,
            RequestId = input.RequestId,
            JobStatus = statusResponse.RequestStatus ?? "UNKNOWN",
            JobStatusDescription = $"Job status: {statusResponse.RequestStatus}",
            CompletedTime = isTerminalState ? DateTime.UtcNow : null,
            LogFileContent = logFileContent,
            OutputFileContent = outputFileContent,
            Output = decodedOutput ?? $"Job {statusResponse.RequestStatus}. Request ID: {statusResponse.ReqstId}",
            Error = isTerminalState && !success ? new Error
            {
                Message = $"Job status: {statusResponse.RequestStatus}. Request ID: {statusResponse.ReqstId}",
                AdditionalInfo = null,
            }
            : null,
        };
    }

    private static bool IsTerminalState(string state)
    {
        return state switch
        {
            "SUCCEEDED" => true,
            "FAILED" => true,
            "WARNING" => true,
            "CANCELLED" => true,
            "ERROR" => true,
            "UNKNOWN" => true,
            _ => false,
        };
    }

    private static string ExtractZipContent(byte[] zipBytes)
    {
        var extractedFiles = new StringBuilder();

        using (var zipStream = new MemoryStream(zipBytes))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
        {
            extractedFiles.AppendLine($"ZIP archive contains {archive.Entries.Count} file(s):");
            extractedFiles.AppendLine();

            foreach (var entry in archive.Entries)
            {
                if (entry.Length == 0)
                    continue;

                extractedFiles.AppendLine($"=== {entry.FullName} ({entry.Length} bytes) ===");

                using (var entryStream = entry.Open())
                using (var reader = new StreamReader(entryStream, Encoding.UTF8))
                {
                    var content = reader.ReadToEnd();
                    extractedFiles.AppendLine(content);
                }

                extractedFiles.AppendLine();
            }
        }

        return extractedFiles.ToString();
    }
}
