using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using Frends.OracleFusion.SubmitEssJob.Definitions;
using Frends.OracleFusion.SubmitEssJob.Helpers;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace Frends.OracleFusion.SubmitEssJob.Tests;

[TestFixture]
public class FunctionalTests
{
    private const string BaseUrl = "https://oracle-test.example.com";
    private const string Username = "testuser";
    private const string Password = "testpass";
    private const string ApiVersion = "latest";
    private const string ExpectedUrl = $"{BaseUrl}/fscmRestApi/resources/{ApiVersion}/erpintegrations";

    [Test]
    public void SubmitEssJob_ValidInput_ReturnsSuccessWithRequestId()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Post, ExpectedUrl)
            .Respond(HttpStatusCode.OK, "application/json", SuccessJson("12345678"));

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var result = OracleFusion.SubmitEssJob(
            BuildValidInput(),
            BuildValidConnection(),
            BuildOptions(),
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.RequestId, Is.EqualTo("12345678"));
        Assert.That(result.Error, Is.Null);
    }

    [Test]
    public void SubmitEssJob_ValidInput_CallsCorrectEndpoint()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .Expect(HttpMethod.Post, ExpectedUrl)
            .Respond(HttpStatusCode.OK, "application/json", SuccessJson());

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        OracleFusion.SubmitEssJob(
            BuildValidInput(),
            BuildValidConnection(),
            BuildOptions(),
            CancellationToken.None);

        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Test]
    public void SubmitEssJob_WithESSParameters_ReturnsSuccess()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Post, ExpectedUrl)
            .Respond(HttpStatusCode.OK, "application/json", SuccessJson());

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var input = BuildValidInput();
        input.ESSParameters = "12345,#NULL,#NULL,#NULL,678,#NULL,#NULL";

        var result = OracleFusion.SubmitEssJob(
            input,
            BuildValidConnection(),
            BuildOptions(),
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
    }

    [Test]
    public void SubmitEssJob_MissingJobPackageName_ThrowsArgumentException()
    {
        var input = BuildValidInput();
        input.JobPackageName = null;

        var ex = Assert.Throws<Exception>(() =>
            OracleFusion.SubmitEssJob(
                input,
                BuildValidConnection(),
                BuildOptions(),
                CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("JobPackageName"));
    }

    [Test]
    public void SubmitEssJob_MissingJobDefName_ThrowsArgumentException()
    {
        var input = BuildValidInput();
        input.JobDefName = null;

        var ex = Assert.Throws<Exception>(() =>
            OracleFusion.SubmitEssJob(
                input,
                BuildValidConnection(),
                BuildOptions(),
                CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("JobDefName"));
    }

    [Test]
    public void SubmitEssJob_Unauthorized_ThrowsException()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Post, ExpectedUrl)
            .Respond(HttpStatusCode.Unauthorized, "application/json", JsonSerializer.Serialize(new { title = "Invalid credentials", status = 401 }));

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var ex = Assert.Throws<Exception>(() =>
            OracleFusion.SubmitEssJob(
                BuildValidInput(),
                BuildValidConnection(),
                BuildOptions(),
                CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Unauthorized"));
    }

    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.Forbidden)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public void SubmitEssJob_HttpError_ThrowsException(HttpStatusCode statusCode)
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Post, ExpectedUrl)
            .Respond(statusCode, "application/json", JsonSerializer.Serialize(new { title = "Error", status = (int)statusCode }));

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var ex = Assert.Throws<Exception>(() =>
            OracleFusion.SubmitEssJob(
                BuildValidInput(),
                BuildValidConnection(),
                BuildOptions(),
                CancellationToken.None));

        Assert.That(ex.Message, Does.Contain(statusCode.ToString()));
    }

    [Test]
    public void SubmitEssJob_MissingRequestIdInResponse_ThrowsException()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Post, ExpectedUrl)
            .Respond(HttpStatusCode.OK, "application/json", JsonSerializer.Serialize(new { operationName = "submitESSJobRequest" }));

        OracleFusion.EssJobClientConstructor = (_, _, _, _) => BuildClient(mockHttp);

        var ex = Assert.Throws<Exception>(() =>
            OracleFusion.SubmitEssJob(
                BuildValidInput(),
                BuildValidConnection(),
                BuildOptions(),
                CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("no request ID returned"));
    }

    [TearDown]
    public void TearDown()
    {
        OracleFusion.EssJobClientConstructor = (baseUrl, username, password, apiVersion)
            => new EssJobClient(baseUrl, username, password, apiVersion);
    }

    private static Input BuildValidInput() => new Input
    {
        JobPackageName = "/oracle/apps/ess/financials/payables/invoices/transactions/",
        JobDefName = "APXIAWRE",
    };

    private static Connection BuildValidConnection() => new Connection
    {
        BaseUrl = BaseUrl,
        Username = Username,
        Password = Password,
        ApiVersion = ApiVersion,
    };

    private static Options BuildOptions() => new Options
    {
        ThrowErrorOnFailure = true,
    };

    private static EssJobClient BuildClient(MockHttpMessageHandler mockHttp) =>
        new EssJobClient(BaseUrl, Username, Password, ApiVersion, mockHttp.ToHttpClient());

    private static string SuccessJson(string requestId = "12345678") =>
        JsonSerializer.Serialize(new { ReqstId = requestId });
}