using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using Frends.OracleFusion.ImportBulkData.Definitions;
using Frends.OracleFusion.ImportBulkData.Helpers;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace Frends.OracleFusion.ImportBulkData.Tests;

[TestFixture]
[NonParallelizable]
public class FunctionalTests
{
    private const string BaseUrl = "https://oracle-test.example.com";
    private const string Username = "testuser";
    private const string Password = "testpass";
    private const string ApiVersion = "latest";
    private const string ExpectedUrl = $"{BaseUrl}/fscmRestApi/resources/{ApiVersion}/erpintegrations";

    [Test]
    public void ImportBulkDataValidInputReturnsSuccess()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Post, ExpectedUrl)
            .Respond(HttpStatusCode.OK, "application/json", SuccessJson("99887766"));

        OracleFusion.FbdiClientConstructor = (_, _, _, _, _) => BuildClient(mockHttp);

        var result = OracleFusion.ImportBulkData(
            BuildValidInput(),
            BuildValidConnection(),
            BuildOptions(),
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.DocumentId, Is.EqualTo("99887766"));
        Assert.That(result.Error, Is.Null);
    }

    [Test]
    public void ImportBulkDataValidInputCallsCorrectEndpoint()
    {
        var mockHttp = new MockHttpMessageHandler();
        var request = mockHttp
            .Expect(HttpMethod.Post, ExpectedUrl)
            .Respond(HttpStatusCode.OK, "application/json", SuccessJson());

        OracleFusion.FbdiClientConstructor = (_, _, _, _, _) => BuildClient(mockHttp);

        OracleFusion.ImportBulkData(
            BuildValidInput(),
            BuildValidConnection(),
            BuildOptions(),
            CancellationToken.None);

        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Test]
    public void ImportBulkDataMultipleFilesStillSingleUpload()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .Expect(HttpMethod.Post, ExpectedUrl)
            .Respond(HttpStatusCode.OK, "application/json", SuccessJson());

        OracleFusion.FbdiClientConstructor = (_, _, _, _, _) => BuildClient(mockHttp);

        var input = BuildValidInput();
        input.Files = new[]
        {
            new File { FileName = "file1.csv", Content = "a,b" },
            new File { FileName = "file2.csv", Content = "c,d" },
            new File { FileName = "file3.csv", Content = "e,f" },
        };

        var result = OracleFusion.ImportBulkData(
            input,
            BuildValidConnection(),
            BuildOptions(),
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Test]
    public void ImportBulkData_Unauthorized_ThrowsHttpRequestException()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Post, ExpectedUrl)
            .Respond(HttpStatusCode.Unauthorized, "application/json", JsonSerializer.Serialize(new { title = "Invalid credentials", status = 401 }));

        OracleFusion.FbdiClientConstructor = (_, _, _, _, _) => BuildClient(mockHttp);

        var ex = Assert.Throws<Exception>(() =>
            OracleFusion.ImportBulkData(
                BuildValidInput(),
                BuildValidConnection(),
                BuildOptions(),
                CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Unauthorized"));
    }

    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.Forbidden)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public void ImportBulkData_HttpError_ThrowsHttpRequestException(HttpStatusCode statusCode)
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Post, ExpectedUrl)
            .Respond(statusCode, "application/json", JsonSerializer.Serialize(new { title = "Error", status = (int)statusCode }));

        OracleFusion.FbdiClientConstructor = (_, _, _, _, _) => BuildClient(mockHttp);

        var ex = Assert.Throws<Exception>(() =>
            OracleFusion.ImportBulkData(
                BuildValidInput(),
                BuildValidConnection(),
                BuildOptions(),
                CancellationToken.None));

        Assert.That(ex.Message, Does.Contain(statusCode.ToString()));
    }

    [Test]
    public void ImportBulkData_MissingDocumentId_ThrowsException()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Post, ExpectedUrl)
            .Respond(HttpStatusCode.OK, "application/json", JsonSerializer.Serialize(new { operationName = "uploadFileToUCM" }));

        OracleFusion.FbdiClientConstructor = (_, _, _, _, _) => BuildClient(mockHttp);

        var ex = Assert.Throws<Exception>(() =>
            OracleFusion.ImportBulkData(
                BuildValidInput(),
                BuildValidConnection(),
                BuildOptions(),
                CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("no document ID returned"));
    }

    [TearDown]
    public void TearDown()
    {
        OracleFusion.FbdiClientConstructor = (baseUrl, username, password, apiVersion, timeout)
            => new FbdiClient(baseUrl, username, password, apiVersion, timeout);
    }

    private static Input BuildValidInput() => new Input
    {
        FileName = "import.zip",
        DocumentAccount = "fin$/payables$/import$",
        Files =
    [
        new File { FileName = "data.csv", Content = "col1,col2\nval1,val2" }
    ],
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
        TimeoutSeconds = 30,
    };

    private static FbdiClient BuildClient(MockHttpMessageHandler mockHttp) =>
        new FbdiClient(BaseUrl, Username, Password, ApiVersion, 30, mockHttp.ToHttpClient());

    private static string SuccessJson(string documentId = "99887766") =>
    JsonSerializer.Serialize(new { documentId });
}
