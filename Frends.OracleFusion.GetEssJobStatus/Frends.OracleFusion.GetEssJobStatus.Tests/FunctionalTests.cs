using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Frends.OracleFusion.GetEssJobStatus.Definitions;
using Frends.OracleFusion.GetEssJobStatus.Helpers;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace Frends.OracleFusion.GetEssJobStatus.Tests;

[TestFixture]
[NonParallelizable]
public class FunctionalTests
{
    private const string BaseUrl = "https://oracle-test.example.com";
    private const string Username = "testuser";
    private const string Password = "testpass";
    private const string ApiVersion = "latest";
    private const string RequestId = "12345678";
    private static readonly string StatusUrl = $"{BaseUrl}/fscmRestApi/resources/{ApiVersion}/erpintegrations?finder=ESSJobStatusRF%3BrequestId%3D{RequestId}";
    private static readonly string LogUrl = $"{BaseUrl}/fscmRestApi/resources/{ApiVersion}/erpintegrations?finder=ESSJobExecutionDetailsRF%3BrequestId%3D{RequestId}%2CfileType%3DLOG";
    private static readonly string OutUrl = $"{BaseUrl}/fscmRestApi/resources/{ApiVersion}/erpintegrations?finder=ESSJobExecutionDetailsRF%3BrequestId%3D{RequestId}%2CfileType%3DOUT";
    private static readonly string OutputUrl = $"{BaseUrl}/fscmRestApi/resources/{ApiVersion}/erpintegrations?finder=ESSJobExecutionDetailsRF%3BrequestId%3D{RequestId}%2CfileType%3DALL";

    [Test]
    public void GetEssJobStatus_SingleCheck_JobSucceeded_ReturnsSuccess()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, StatusUrl)
            .Respond(HttpStatusCode.OK, "application/json", StatusJson("SUCCEEDED"));

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var result = OracleFusion.GetEssJobStatus(
            BuildValidInput(),
            BuildValidConnection(),
            BuildOptions(waitForCompletion: false),
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.IsCompleted, Is.True);
        Assert.That(result.JobStatus, Is.EqualTo("SUCCEEDED"));
        Assert.That(result.RequestId, Is.EqualTo(RequestId));
        Assert.That(result.Error, Is.Null);
    }

    [Test]
    public void GetEssJobStatus_SingleCheck_JobStillRunning_ReturnsNotCompleted()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, StatusUrl)
            .Respond(HttpStatusCode.OK, "application/json", StatusJson("RUNNING"));

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var result = OracleFusion.GetEssJobStatus(
            BuildValidInput(),
            BuildValidConnection(),
            BuildOptions(waitForCompletion: false),
            CancellationToken.None);

        Assert.That(result.IsCompleted, Is.False);
        Assert.That(result.Success, Is.False);
        Assert.That(result.JobStatus, Is.EqualTo("RUNNING"));
        Assert.That(result.Error, Is.Null);
    }

    [Test]
    public void GetEssJobStatus_SingleCheck_JobWarning_ReturnsSuccess()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, StatusUrl)
            .Respond(HttpStatusCode.OK, "application/json", StatusJson("WARNING"));

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var result = OracleFusion.GetEssJobStatus(
            BuildValidInput(),
            BuildValidConnection(),
            BuildOptions(waitForCompletion: false),
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.IsCompleted, Is.True);
        Assert.That(result.JobStatus, Is.EqualTo("WARNING"));
    }

    [Test]
    public void GetEssJobStatus_SingleCheck_JobFailed_ThrowsException()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, StatusUrl)
            .Respond(HttpStatusCode.OK, "application/json", StatusJson("FAILED"));

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var ex = Assert.Throws<Exception>(() =>
            OracleFusion.GetEssJobStatus(
                BuildValidInput(),
                BuildValidConnection(),
                BuildOptions(waitForCompletion: false),
                CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("FAILED"));
    }

    [Test]
    public void GetEssJobStatus_WaitForCompletion_JobSucceedsAfterPolling_ReturnsSuccess()
    {
        var responses = new Queue<string>(["RUNNING", "RUNNING", "SUCCEEDED"]);
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, StatusUrl)
            .Respond(() =>
            {
                var status = responses.Dequeue();
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(StatusJson(status), System.Text.Encoding.UTF8, "application/json"),
                });
            });

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var result = OracleFusion.GetEssJobStatus(
            BuildValidInput(),
            BuildValidConnection(),
            BuildOptions(waitForCompletion: true, timeoutMinutes: 1, pollingIntervalSeconds: 0),
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.IsCompleted, Is.True);
        Assert.That(result.JobStatus, Is.EqualTo("SUCCEEDED"));
    }

    [Test]
    public void GetEssJobStatus_WaitForCompletion_JobSucceedsImmediately_ReturnsSuccess()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, StatusUrl)
            .Respond(HttpStatusCode.OK, "application/json", StatusJson("SUCCEEDED"));
        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var result = OracleFusion.GetEssJobStatus(
            BuildValidInput(),
            BuildValidConnection(),
            BuildOptions(waitForCompletion: true, timeoutMinutes: 1, pollingIntervalSeconds: 0),
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.IsCompleted, Is.True);
    }

    [Test]
    public void GetEssJobStatus_WaitForCompletion_Timeout_ThrowsTimeoutException()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, StatusUrl)
            .Respond(HttpStatusCode.OK, "application/json", StatusJson("RUNNING"));

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var ex = Assert.Throws<Exception>(() =>
            OracleFusion.GetEssJobStatus(
                BuildValidInput(),
                BuildValidConnection(),
                BuildOptions(waitForCompletion: true, timeoutMinutes: 0, pollingIntervalSeconds: 0),
                CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("timeout").Or.Contain("Timeout").Or.Contain("did not complete"));
    }

    [Test]
    public void GetEssJobStatus_Unauthorized_ThrowsException()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, StatusUrl)
            .Respond(HttpStatusCode.Unauthorized, "application/json", JsonSerializer.Serialize(new { title = "Invalid credentials", status = 401 }));

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var ex = Assert.Throws<Exception>(() =>
            OracleFusion.GetEssJobStatus(
                BuildValidInput(),
                BuildValidConnection(),
                BuildOptions(),
                CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Unauthorized"));
    }

    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.Forbidden)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public void GetEssJobStatus_HttpError_ThrowsException(HttpStatusCode statusCode)
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, StatusUrl)
            .Respond(statusCode, "application/json", JsonSerializer.Serialize(new { title = "Error", status = (int)statusCode }));

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var ex = Assert.Throws<Exception>(() =>
            OracleFusion.GetEssJobStatus(
                BuildValidInput(),
                BuildValidConnection(),
                BuildOptions(),
                CancellationToken.None));

        Assert.That(ex.Message, Does.Contain(statusCode.ToString()));
    }

    [Test]
    public void GetEssJobStatus_MissingRequestId_ThrowsArgumentException()
    {
        var input = BuildValidInput();
        input.RequestId = null;

        var ex = Assert.Throws<Exception>(() =>
            OracleFusion.GetEssJobStatus(
                input,
                BuildValidConnection(),
                BuildOptions(),
                CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("RequestId"));
    }

    [Test]
    public void GetEssJobStatus_SingleCheck_JobCancelled_ReturnsCompletedNotSuccess()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, StatusUrl)
            .Respond(HttpStatusCode.OK, "application/json", StatusJson("CANCELLED"));

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var result = OracleFusion.GetEssJobStatus(
            BuildValidInput(),
            BuildValidConnection(),
            BuildOptions(waitForCompletion: false, throwErrorOnFailure: false),
            CancellationToken.None);

        Assert.That(result.IsCompleted, Is.True);
        Assert.That(result.Success, Is.False);
        Assert.That(result.JobStatus, Is.EqualTo("CANCELLED"));
        Assert.That(result.Error, Is.Not.Null);
    }

    [Test]
    public void GetEssJobStatus_IncludeLogFile_JobSucceeded_LogFileContentPopulated()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, StatusUrl)
            .Respond(HttpStatusCode.OK, "application/json", StatusJson("SUCCEEDED"));
        mockHttp
            .When(HttpMethod.Get, LogUrl)
            .Respond(HttpStatusCode.OK, "application/json", OutputJson("SUCCEEDED", "LOG", "aGVsbG8="));

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var options = BuildOptions(waitForCompletion: false);
        options.IncludeLogFile = true;

        var result = OracleFusion.GetEssJobStatus(
            BuildValidInput(),
            BuildValidConnection(),
            options,
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.LogFileContent, Is.EqualTo("aGVsbG8="));
        Assert.That(result.OutputFileContent, Is.Null);
    }

    [Test]
    public void GetEssJobStatus_IncludeOutputFile_JobSucceeded_OutputFileContentPopulated()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, StatusUrl)
            .Respond(HttpStatusCode.OK, "application/json", StatusJson("SUCCEEDED"));
        mockHttp
            .When(HttpMethod.Get, OutUrl)
            .Respond(HttpStatusCode.OK, "application/json", OutputJson("SUCCEEDED", "OUT", "aGVsbG8="));

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var options = BuildOptions(waitForCompletion: false);
        options.IncludeOutputFile = true;

        var result = OracleFusion.GetEssJobStatus(
            BuildValidInput(),
            BuildValidConnection(),
            options,
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.OutputFileContent, Is.EqualTo("aGVsbG8="));
        Assert.That(result.LogFileContent, Is.Null);
    }

    [Test]
    public void GetEssJobStatus_IncludeLogAndOutputFile_JobSucceeded_BothContentPopulated()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, StatusUrl)
            .Respond(HttpStatusCode.OK, "application/json", StatusJson("SUCCEEDED"));
        mockHttp
            .When(HttpMethod.Get, OutputUrl)
            .Respond(HttpStatusCode.OK, "application/json", OutputJson("SUCCEEDED", "ALL", "aGVsbG8="));

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var options = BuildOptions(waitForCompletion: false);
        options.IncludeLogFile = true;
        options.IncludeOutputFile = true;

        var result = OracleFusion.GetEssJobStatus(
            BuildValidInput(),
            BuildValidConnection(),
            options,
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.LogFileContent, Is.EqualTo("aGVsbG8="));
        Assert.That(result.OutputFileContent, Is.EqualTo("aGVsbG8="));
    }

    [Test]
    public void GetEssJobStatus_IncludeLogFile_JobStillRunning_SkipsDownload()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, StatusUrl)
            .Respond(HttpStatusCode.OK, "application/json", StatusJson("RUNNING"));

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var options = BuildOptions(waitForCompletion: false);
        options.IncludeLogFile = true;

        var result = OracleFusion.GetEssJobStatus(
            BuildValidInput(),
            BuildValidConnection(),
            options,
            CancellationToken.None);

        Assert.That(result.IsCompleted, Is.False);
        Assert.That(result.LogFileContent, Is.Null);
        Assert.That(result.OutputFileContent, Is.Null);
    }

    [Test]
    public void GetEssJobStatus_IncludeLogFile_DownloadFails_OutputContainsErrorMessage()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, StatusUrl)
            .Respond(HttpStatusCode.OK, "application/json", StatusJson("SUCCEEDED"));
        mockHttp
            .When(HttpMethod.Get, LogUrl)
            .Respond(HttpStatusCode.InternalServerError, "application/json", "{}");

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var options = BuildOptions(waitForCompletion: false);
        options.IncludeLogFile = true;

        var result = OracleFusion.GetEssJobStatus(
            BuildValidInput(),
            BuildValidConnection(),
            options,
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.LogFileContent, Is.Null);
        Assert.That(result.Output, Does.Contain("Failed to retrieve job output"));
    }

    [TearDown]
    public void TearDown()
    {
        OracleFusion.EssJobClientConstructor = (baseUrl, username, password, apiVersion)
            => new EssJobClient(baseUrl, username, password, apiVersion);
    }

    private static Input BuildValidInput() => new Input
    {
        RequestId = RequestId,
    };

    private static Connection BuildValidConnection() => new Connection
    {
        BaseUrl = BaseUrl,
        Username = Username,
        Password = Password,
        ApiVersion = ApiVersion,
    };

    private static Options BuildOptions(
    bool waitForCompletion = false,
    int timeoutMinutes = 1,
    int pollingIntervalSeconds = 0,
    bool throwErrorOnFailure = true) => new Options
    {
        WaitForCompletion = waitForCompletion,
        TimeoutMinutes = timeoutMinutes,
        PollingIntervalSeconds = pollingIntervalSeconds,
        ThrowErrorOnFailure = throwErrorOnFailure,
    };

    private static EssJobClient BuildClient(MockHttpMessageHandler mockHttp) =>
        new EssJobClient(BaseUrl, Username, Password, ApiVersion, mockHttp.ToHttpClient());

    private static string StatusJson(string status, string requestId = RequestId)
    {
        return JsonSerializer.Serialize(new
        {
            items = new[]
            {
                new { ReqstId = requestId, RequestStatus = status },
            },
        });
    }

    private static string OutputJson(string status, string fileType, string documentContent) =>
        JsonSerializer.Serialize(new
        {
            items = new[]
            {
                new
                {
                    ReqstId = RequestId,
                    RequestStatus = status,
                    FileType = fileType,
                    DocumentContent = documentContent,
                },
            },
        });
}
