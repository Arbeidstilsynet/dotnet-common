using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Arbeidstilsynet.Common.Extensions;

internal class RequestValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context
                .ModelState.Where(entry => entry.Value?.Errors.Any() == true)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var problemDetails = new ValidationProblemDetails(errors)
            {
                Title = "Validation Failed",
                Status = StatusCodes.Status400BadRequest,
            };

            context.Result = new BadRequestObjectResult(problemDetails);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
