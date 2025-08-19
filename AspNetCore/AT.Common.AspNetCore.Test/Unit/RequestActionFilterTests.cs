using Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Shouldly;
using RouteData = Microsoft.AspNetCore.Routing.RouteData;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.Test.Unit;

public class RequestActionFilterTests
{
    private readonly RequestValidationFilter _sut = new();

    [Fact]
    public void OnActionExecuting_ModelStateIsValid_NothingHappens()
    {
        // Arrange
        var actionContext = CreateMockActionExecutingContext();

        // Act
        _sut.OnActionExecuting(actionContext);

        // Assert
        actionContext.ModelState.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void OnActionExecuting_ModelStateIsInvalid_ReturnsBadRequest_WithValidationProblemDetails()
    {
        // Arrange
        var actionContext = CreateMockActionExecutingContext();
        actionContext.ModelState.AddModelError("TestKey", "Test error message");
        actionContext.ModelState.AddModelError("TestKey2", "Second error message");

        // Act
        _sut.OnActionExecuting(actionContext);

        // Assert
        actionContext.Result.ShouldBeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)actionContext.Result;
        badRequestResult.Value.ShouldBeOfType<ValidationProblemDetails>();
        var problemDetails = (ValidationProblemDetails)badRequestResult.Value;

        problemDetails.Errors.ShouldContainKey("TestKey");
        problemDetails.Errors["TestKey"].ShouldContain("Test error message");

        problemDetails.Errors.ShouldContainKey("TestKey2");
        problemDetails.Errors["TestKey2"].ShouldContain("Second error message");
    }

    private static ActionExecutingContext CreateMockActionExecutingContext()
    {
        return new ActionExecutingContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: null!
        );
    }
}
