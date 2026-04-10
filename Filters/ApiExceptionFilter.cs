namespace api_aggregations.Filters;

using api_aggregations.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

public sealed class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;
    private readonly IWebHostEnvironment _env;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public void OnException(ExceptionContext context)
    {
        var traceId = context.HttpContext.TraceIdentifier;

        if (context.Exception is DbUpdateConcurrencyException)
        {
            LogMapped(context.Exception, StatusCodes.Status409Conflict, traceId);

            context.Result = new ObjectResult(new ProblemDetails
            {
                Title = "Conflict",
                Detail = "The resource was updated by another request. Please retry.",
                Status = StatusCodes.Status409Conflict,
                Instance = traceId
            })
            {
                StatusCode = StatusCodes.Status409Conflict
            };

            context.ExceptionHandled = true;
            return;
        }

        if (context.Exception is DbUpdateException dbUpdateException)
        {
            var (statusCode, title, detail) = MapDbUpdateException(dbUpdateException);
            LogMapped(context.Exception, statusCode, traceId);

            context.Result = new ObjectResult(new ProblemDetails
            {
                Title = title,
                Detail = detail,
                Status = statusCode,
                Instance = traceId
            })
            {
                StatusCode = statusCode
            };

            context.ExceptionHandled = true;
            return;
        }

        if (context.Exception is ApiException apiException)
        {
            _logger.LogWarning(
                context.Exception,
                "Handled API exception ({StatusCode}). TraceId={TraceId}",
                apiException.StatusCode,
                traceId
            );

            context.Result = new ObjectResult(new ProblemDetails
            {
                Title = "Request failed",
                Detail = apiException.Message,
                Status = apiException.StatusCode,
                Instance = traceId
            })
            {
                StatusCode = apiException.StatusCode
            };

            context.ExceptionHandled = true;
            return;
        }

        _logger.LogError(context.Exception, "Unhandled exception. TraceId={TraceId}", traceId);

        context.Result = new ObjectResult(new ProblemDetails
        {
            Title = "Unexpected error",
            Detail = _env.IsDevelopment() ? context.Exception.ToString() : "An unexpected error occurred.",
            Status = StatusCodes.Status500InternalServerError,
            Instance = traceId
        })
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };

        context.ExceptionHandled = true;
    }

    private void LogMapped(Exception exception, int statusCode, string traceId)
    {
        _logger.LogWarning(exception, "Mapped exception to HTTP {StatusCode}. TraceId={TraceId}", statusCode, traceId);
    }

    private (int StatusCode, string Title, string Detail) MapDbUpdateException(DbUpdateException exception)
    {
        if (exception.InnerException is SqlException sqlException)
        {
            return sqlException.Number switch
            {
                547 => (
                    StatusCodes.Status400BadRequest,
                    "Constraint violation",
                    _env.IsDevelopment()
                        ? sqlException.Message
                        : "A referenced entity was not found or a constraint was violated."
                ),
                515 => (
                    StatusCodes.Status400BadRequest,
                    "Missing required field",
                    _env.IsDevelopment()
                        ? sqlException.Message
                        : "One or more required fields are missing."
                ),
                2627 or 2601 => (
                    StatusCodes.Status409Conflict,
                    "Duplicate key",
                    _env.IsDevelopment()
                        ? sqlException.Message
                        : "A record with the same key already exists."
                ),
                _ => (
                    StatusCodes.Status400BadRequest,
                    "Invalid data",
                    _env.IsDevelopment()
                        ? sqlException.Message
                        : "The request data is invalid."
                )
            };
        }

        return (
            StatusCodes.Status400BadRequest,
            "Invalid data",
            _env.IsDevelopment() ? exception.ToString() : "The request data is invalid."
        );
    }
}

