using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Frends.OracleFusion.RunEssJob.Definitions;
using Frends.OracleFusion.RunEssJob.Helpers;

namespace Frends.OracleFusion.RunEssJob;

/// <summary>
/// Task Class for OracleFusion operations.
/// </summary>
public static class OracleFusion
{
    /// <summary>
    /// Submits an Oracle Fusion ESS job, monitors its execution, and retrieves output/logs.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-OracleFusion-RunEssJob)
    /// </summary>
    /// <param name="input">Essential parameters including job definition and parameters.</param>
    /// <param name="connection">Connection parameters for Oracle Fusion authentication.</param>
    /// <param name="options">Additional parameters for polling, timeout, and output retrieval.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, long RequestId, string JobStatus, string JobStatusDescription, DateTime? CompletedTime, string LogFileContent, string OutputFileContent, string Output, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result RunEssJob(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            return RunEssJobAsync(input, connection, options, cancellationToken).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }

    private static async Task<Result> RunEssJobAsync(
        Input input,
        Connection connection,
        Options options,
        CancellationToken cancellationToken)
    {
        // Validate required input parameters
        if (string.IsNullOrWhiteSpace(input.JobPackageName))
            throw new ArgumentException("JobPackageName is required.", nameof(input));

        if (string.IsNullOrWhiteSpace(input.JobDefName))
            throw new ArgumentException("JobDefName is required.", nameof(input));

        if (string.IsNullOrWhiteSpace(connection.BaseUrl))
            throw new ArgumentException("BaseUrl is required.", nameof(connection));

        if (string.IsNullOrWhiteSpace(connection.Username))
            throw new ArgumentException("Username is required.", nameof(connection));

        if (string.IsNullOrWhiteSpace(connection.Password))
            throw new ArgumentException("Password is required.", nameof(connection));

        cancellationToken.ThrowIfCancellationRequested();

        using var client = new EssJobClient(connection.BaseUrl, connection.Username, connection.Password, connection.ApiVersion);

        // Step 1: Submit the ESS job
        var requestId = await client.SubmitJobAsync(input, cancellationToken);

        // Step 2: Poll for job completion
        var timeoutTime = DateTime.UtcNow.AddMinutes(options.TimeoutMinutes);
        JobStatusResponse statusResponse = null;
        var isTerminalState = false;

        while (!isTerminalState && DateTime.UtcNow < timeoutTime)
        {
            cancellationToken.ThrowIfCancellationRequested();

            statusResponse = await client.GetJobStatusAsync(requestId, cancellationToken);

            // Check if job is in a terminal state
            isTerminalState = IsTerminalState(statusResponse.Status);

            if (!isTerminalState)
            {
                await Task.Delay(TimeSpan.FromSeconds(options.PollingIntervalSeconds), cancellationToken);
            }
        }

        if (!isTerminalState)
        {
            throw new TimeoutException(
                $"Job did not complete within the timeout period of {options.TimeoutMinutes} minutes. " +
                $"Last known status: {statusResponse?.Status}");
        }

        // Step 3: Download output/logs if requested and job succeeded
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
                    requestId,
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

                // Try to decode and extract ZIP content for convenience
                if (!string.IsNullOrEmpty(outputResponse.DocumentContent))
                {
                    try
                    {
                        var zipBytes = Convert.FromBase64String(outputResponse.DocumentContent);
                        var extractedContent = ExtractZipContent(zipBytes);
                        decodedOutput = extractedContent;
                    }
                    catch (Exception ex)
                    {
                        decodedOutput = $"Failed to extract ZIP content: {ex.Message}. ZIP file size: {outputResponse.DocumentContent.Length} chars (base64).";
                    }
                }
            }
            catch (Exception ex)
            {
                // If output retrieval fails, log it but don't fail the entire task
                decodedOutput = $"Failed to retrieve job output: {ex.Message}";
            }
        }

        // Determine overall success
        var success = statusResponse.Status == "SUCCEEDED" || statusResponse.Status == "WARNING";

        // If job failed, throw an exception if configured to do so
        if (!success && options.ThrowErrorOnFailure)
        {
            throw new Exception(
                $"ESS job failed. Status: {statusResponse.Status}, Request ID: {statusResponse.ReqstId}");
        }

        return new Result
        {
            Success = success,
            RequestId = requestId,
            JobStatus = statusResponse.RequestStatus ?? "UNKNOWN",
            JobStatusDescription = $"Job status: {statusResponse.RequestStatus}",
            CompletedTime = null,
            LogFileContent = logFileContent,
            OutputFileContent = outputFileContent,
            Output = decodedOutput ?? $"Job {statusResponse.RequestStatus}. Request ID: {statusResponse.ReqstId}",
            Error = success ? null : new Error
            {
                Message = $"Job status: {statusResponse.RequestStatus}. Request ID: {statusResponse.ReqstId}",
                AdditionalInfo = null,
            },
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