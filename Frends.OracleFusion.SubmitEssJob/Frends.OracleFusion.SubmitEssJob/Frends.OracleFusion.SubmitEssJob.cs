using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Frends.OracleFusion.SubmitEssJob.Definitions;
using Frends.OracleFusion.SubmitEssJob.Helpers;

namespace Frends.OracleFusion.SubmitEssJob;

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
    /// Submits an Oracle Fusion ESS job and immediately returns the assigned Request ID.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-OracleFusion-SubmitEssJob)
    /// </summary>
    /// <param name="input">Essential parameters including job definition and parameters.</param>
    /// <param name="connection">Connection parameters for Oracle Fusion authentication.</param>
    /// <param name="options">Additional parameters for polling, timeout, and output retrieval.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string RequestId, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result SubmitEssJob(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            return SubmitEssJobAsync(input, connection, options, cancellationToken).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }

    private static async Task<Result> SubmitEssJobAsync(
        Input input,
        Connection connection,
        Options options,
        CancellationToken cancellationToken)
    {
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

        using var client = EssJobClientConstructor(connection.BaseUrl, connection.Username, connection.Password, connection.ApiVersion);

        var requestId = await client.SubmitJobAsync(input, cancellationToken);

        return new Result
        {
            Success = true,
            RequestId = requestId,
            Error = null,
        };
    }
}