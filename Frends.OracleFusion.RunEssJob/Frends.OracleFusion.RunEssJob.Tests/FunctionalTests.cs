using System;
using System.Collections.Generic;
using System.Threading;
using Frends.OracleFusion.RunEssJob.Definitions;
using NUnit.Framework;

namespace Frends.OracleFusion.RunEssJob.Tests;

[TestFixture]
public class FunctionalTests : TestBase
{
    // NOTE: These tests require a valid Oracle Fusion environment.
    // Set the following environment variables in .env file:
    // ESS_BASE_URL=https://your-instance.fa.oraclecloud.com
    // ESS_USERNAME=your-username
    // ESS_PASSWORD=your-password
    // ESS_JOB_PACKAGE_NAME=/oracle/apps/ess/financials/payables/invoices/transactions/
    // ESS_JOB_DEF_NAME=APXIAWRE
    [Test]
    [Ignore("Requires Oracle Fusion environment and credentials")]
    public void TestSubmitAndWaitForJobSuccess()
    {
        var input = new Input
        {
            JobPackageName = Environment.GetEnvironmentVariable("ESS_JOB_PACKAGE_NAME"),
            JobDefName = Environment.GetEnvironmentVariable("ESS_JOB_DEF_NAME"),
            ESSParameters = string.Empty,
        };

        var connection = new Connection
        {
            BaseUrl = Environment.GetEnvironmentVariable("ESS_BASE_URL"),
            Username = Environment.GetEnvironmentVariable("ESS_USERNAME"),
            Password = Environment.GetEnvironmentVariable("ESS_PASSWORD"),
            ApiVersion = "latest",
        };

        var options = new Options
        {
            PollingIntervalSeconds = 5,
            TimeoutMinutes = 10,
            IncludeLogFile = true,
            IncludeOutputFile = true,
            ThrowErrorOnFailure = true,
            ErrorMessageOnFailure = string.Empty,
        };

        var result = OracleFusion.RunEssJob(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.RequestId, Is.Not.Null);
        Assert.That(result.JobStatus, Is.EqualTo("SUCCEEDED").Or.EqualTo("WARNING"));
        Assert.That(result.Output, Is.Not.Null);
    }

    [Test]
    [Ignore("Requires Oracle Fusion environment and credentials")]
    public void TestSubmitJobWithParameters()
    {
        var input = new Input
        {
            JobPackageName = Environment.GetEnvironmentVariable("ESS_JOB_PACKAGE_NAME"),
            JobDefName = Environment.GetEnvironmentVariable("ESS_JOB_DEF_NAME"),
            ESSParameters = "12345,#NULL,#NULL,#NULL,678,#NULL,#NULL",
        };

        var connection = new Connection
        {
            BaseUrl = Environment.GetEnvironmentVariable("ESS_BASE_URL"),
            Username = Environment.GetEnvironmentVariable("ESS_USERNAME"),
            Password = Environment.GetEnvironmentVariable("ESS_PASSWORD"),
            ApiVersion = "latest",
        };

        var options = new Options
        {
            PollingIntervalSeconds = 5,
            TimeoutMinutes = 10,
            IncludeLogFile = false,
            IncludeOutputFile = false,
            ThrowErrorOnFailure = true,
            ErrorMessageOnFailure = string.Empty,
        };

        var result = OracleFusion.RunEssJob(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.RequestId, Is.Not.Null);
    }

    [Test]
    public void TestMissingJobPackageName()
    {
        var input = new Input
        {
            JobPackageName = string.Empty,
            JobDefName = "TESTJOB",
        };

        var connection = new Connection
        {
            BaseUrl = "https://test.fa.oraclecloud.com",
            Username = "testuser",
            Password = "testpass",
        };

        var options = new Options
        {
            ThrowErrorOnFailure = true,
        };

        var ex = Assert.Throws<Exception>(() =>
            OracleFusion.RunEssJob(input, connection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("JobPackageName"));
    }

    [Test]
    public void TestMissingJobDefName()
    {
        var input = new Input
        {
            JobPackageName = "/oracle/apps/test/",
            JobDefName = string.Empty,
        };

        var connection = new Connection
        {
            BaseUrl = "https://test.fa.oraclecloud.com",
            Username = "testuser",
            Password = "testpass",
        };

        var options = new Options
        {
            ThrowErrorOnFailure = true,
        };

        var ex = Assert.Throws<Exception>(() =>
            OracleFusion.RunEssJob(input, connection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("JobDefName"));
    }

    [Test]
    public void TestMissingBaseUrl()
    {
        var input = new Input
        {
            JobPackageName = "/oracle/apps/test/",
            JobDefName = "TESTJOB",
        };

        var connection = new Connection
        {
            BaseUrl = string.Empty,
            Username = "testuser",
            Password = "testpass",
        };

        var options = new Options
        {
            ThrowErrorOnFailure = true,
        };

        var ex = Assert.Throws<Exception>(() =>
            OracleFusion.RunEssJob(input, connection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("BaseUrl"));
    }

    [Test]
    [Ignore("Requires Oracle Fusion environment - will test cancellation")]
    public void TestCancellation()
    {
        var input = new Input
        {
            JobPackageName = Environment.GetEnvironmentVariable("ESS_JOB_PACKAGE_NAME"),
            JobDefName = Environment.GetEnvironmentVariable("ESS_JOB_DEF_NAME"),
        };

        var connection = new Connection
        {
            BaseUrl = Environment.GetEnvironmentVariable("ESS_BASE_URL"),
            Username = Environment.GetEnvironmentVariable("ESS_USERNAME"),
            Password = Environment.GetEnvironmentVariable("ESS_PASSWORD"),
        };

        var options = new Options
        {
            PollingIntervalSeconds = 5,
            TimeoutMinutes = 10,
            ThrowErrorOnFailure = true,
        };

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        Assert.Throws<OperationCanceledException>(() =>
            OracleFusion.RunEssJob(input, connection, options, cts.Token));
    }
}
