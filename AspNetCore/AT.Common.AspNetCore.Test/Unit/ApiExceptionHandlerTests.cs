using System.Net;
using Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;
using Arbeidstilsynet.Common.AspNetCore.Extensions.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.Test.Unit;

public class ApiExceptionHandlerTests
{
    [Fact]
    public async Task CreateExceptionHandler_DefaultMapping_ArgumentExceptionProduces400BadRequest()
    {
        // Arrange
        var context = SetupContext(new ArgumentException());

        // Act
        var handler = ApiExceptionHandler.CreateExceptionHandler(new ExceptionHandlingOptions());
        await handler(context);

        // Assert
        context.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task CreateExceptionHandler_DefaultMapping_FormatExceptionProduces400BadRequest()
    {
        // Arrange
        var context = SetupContext(new FormatException());

        // Act
        var handler = ApiExceptionHandler.CreateExceptionHandler(new ExceptionHandlingOptions());
        await handler(context);

        // Assert
        context.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task CreateExceptionHandler_DefaultMapping_BadHttpRequestExceptionProduces400BadRequest()
    {
        // Arrange
        var context = SetupContext(new BadHttpRequestException("Bad request"));

        // Act
        var handler = ApiExceptionHandler.CreateExceptionHandler(new ExceptionHandlingOptions());
        await handler(context);

        // Assert
        context.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task CreateExceptionHandler_DefaultMapping_UnmappedExceptionProduces500InternalServerError()
    {
        // Arrange
        var context = SetupContext(new UnmappedException("Unknown"));

        // Act
        var handler = ApiExceptionHandler.CreateExceptionHandler(new ExceptionHandlingOptions());
        await handler(context);

        // Assert
        context.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, 404)]
    [InlineData(HttpStatusCode.BadRequest, 400)]
    public async Task CreateExceptionHandler_CustomMapping_CustomExceptionProducesConfiguredStatus(
        HttpStatusCode statusCode,
        int expectedValue
    )
    {
        // Arrange
        var context = SetupContext(new CustomException("Custom error"));
        var options = new ExceptionHandlingOptions().AddExceptionMapping<CustomException>(
            statusCode
        );

        // Act
        var handler = ApiExceptionHandler.CreateExceptionHandler(options);
        await handler(context);

        // Assert
        context.Response.StatusCode.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, 404)]
    [InlineData(HttpStatusCode.Gone, 410)]
    public async Task CreateExceptionHandler_OverwriteMapping_ProducesConfiguredStatus(
        HttpStatusCode statusCode,
        int expectedValue
    )
    {
        // Arrange
        var context = SetupContext(new ArgumentException("Custom error"));
        var options = new ExceptionHandlingOptions().AddExceptionMapping<ArgumentException>(
            statusCode
        );

        // Act
        var handler = ApiExceptionHandler.CreateExceptionHandler(options);
        await handler(context);

        // Assert
        context.Response.StatusCode.ShouldBe(expectedValue);
    }

    [Fact]
    public async Task CreateExceptionHandler_NullException_Produces500InternalServerError()
    {
        // Arrange
        var context = SetupContext<Exception>(null!);

        // Act
        var handler = ApiExceptionHandler.CreateExceptionHandler(new ExceptionHandlingOptions());
        await handler(context);

        // Assert
        context.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
    }

    [Theory]
    [InlineData("Should yield 424", 424)]
    [InlineData("Something else", 501)]
    public async Task CreateExceptionHandler_DynamicMapping_ProducesStatusBasedOnExceptionContent(
        string exceptionContents,
        int expectedStatusCode
    )
    {
        // Arrange
        var context = SetupContext<Exception>(new ArgumentException(exceptionContents));
        var exceptionHandlingOptions =
            new ExceptionHandlingOptions().AddExceptionMapping<ArgumentException>(ex =>
            {
                if (ex!.Message.Contains("Should yield 424"))
                {
                    return HttpStatusCode.FailedDependency;
                }

                return HttpStatusCode.NotImplemented;
            });

        // Act
        var handler = ApiExceptionHandler.CreateExceptionHandler(exceptionHandlingOptions);
        await handler(context);

        // Assert
        context.Response.StatusCode.ShouldBe(expectedStatusCode);
    }

    private static HttpContext SetupContext<TException>(TException exception)
        where TException : Exception
    {
        var exceptionHandlerPathFeature = Substitute.For<IExceptionHandlerPathFeature>();
        exceptionHandlerPathFeature.Error.Returns(exception);

        var context = new DefaultHttpContext();
        context.Features.Set(exceptionHandlerPathFeature);
        context.Response.Body = new MemoryStream();

        return context;
    }

    private class CustomException : Exception
    {
        public CustomException(string message)
            : base(message) { }
    }

    private class UnmappedException : Exception
    {
        public UnmappedException(string message)
            : base(message) { }
    }
}
