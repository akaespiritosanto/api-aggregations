namespace api_aggregations.Filters;

using api_aggregations.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public sealed class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ApiException apiException)
        {
            context.Result = new ObjectResult(new ProblemDetails
            {
                Title = "Request failed",
                Detail = apiException.Message,
                Status = apiException.StatusCode
            })
            {
                StatusCode = apiException.StatusCode
            };

            context.ExceptionHandled = true;
            return;
        }

        context.Result = new ObjectResult(new ProblemDetails
        {
            Title = "Unexpected error",
            Detail = "An unexpected error occurred.",
            Status = StatusCodes.Status500InternalServerError
        })
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };

        context.ExceptionHandled = true;
    }
}

