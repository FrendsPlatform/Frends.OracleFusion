using System;
using System.Threading;
using Frends.OracleFusion.RunEssJob.Definitions;
using NUnit.Framework;

namespace Frends.OracleFusion.RunEssJob.Tests;

[TestFixture]
public class ErrorHandlerTest
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var ex = Assert.Throws<Exception>(() =>
           OracleFusion.RunEssJob(DefaultInput(), DefaultConnection(), DefaultOptions(), CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Does.Contain("JobPackageName"));
    }

    [Test]
    public void Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        var result = OracleFusion.RunEssJob(DefaultInput(), DefaultConnection(), options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = DefaultOptions();
        options.ErrorMessageOnFailure = CustomErrorMessage;
        options.ThrowErrorOnFailure = false;
        var result = OracleFusion.RunEssJob(DefaultInput(), DefaultConnection(), options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error.Message, Contains.Substring(CustomErrorMessage));
    }

    private static Input DefaultInput() => new()
    {
        JobPackageName = string.Empty, // Invalid value to cause an exception
        JobDefName = "TestJob",
    };

    private static Connection DefaultConnection() => new()
    {
        BaseUrl = "https://test.fa.oraclecloud.com",
        Username = "testuser",
        Password = "testpass",
    };

    private static Options DefaultOptions() => new()
    {
        ThrowErrorOnFailure = true,
        ErrorMessageOnFailure = string.Empty,
    };
}
