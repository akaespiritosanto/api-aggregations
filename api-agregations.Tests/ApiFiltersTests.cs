namespace api_agregations.Tests;

using api_aggregations.Exceptions;
using api_aggregations.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

public class ApiFiltersTests
{
    [Fact]
    public async Task ApiKeyAuthFilter_WhenApiKeyIsMissing_ReturnsUnauthorized()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["API_KEY"] = "secret-key" })
            .Build();

        var filter = new ApiKeyAuthFilter(configuration);
        var context = CreateAuthorizationContext();

        await filter.OnAuthorizationAsync(context);

        var result = Assert.IsType<UnauthorizedObjectResult>(context.Result);
        var details = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.Equal("Unauthorized", details.Title);
    }

    [Fact]
    public async Task ApiKeyAuthFilter_WhenHeaderMatches_AllowsRequest()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["API_KEY"] = "secret-key" })
            .Build();

        var filter = new ApiKeyAuthFilter(configuration);
        var context = CreateAuthorizationContext();
        context.HttpContext.Request.Headers["X-API-KEY"] = "secret-key";

        await filter.OnAuthorizationAsync(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void ApiExceptionFilter_WhenApiExceptionIsThrown_ReturnsMappedProblemDetails()
    {
        var filter = new ApiExceptionFilter(
            NullLogger<ApiExceptionFilter>.Instance,
            new FakeEnvironment { EnvironmentName = Environments.Production });

        var context = CreateExceptionContext(new BadRequestException("Invalid request"));

        filter.OnException(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var details = Assert.IsType<ProblemDetails>(result.Value);

        Assert.True(context.ExceptionHandled);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Request failed", details.Title);
        Assert.Equal("Invalid request", details.Detail);
    }

    [Fact]
    public void ApiExceptionFilter_WhenUnexpectedExceptionIsThrownInProduction_HidesInternalDetails()
    {
        var filter = new ApiExceptionFilter(
            NullLogger<ApiExceptionFilter>.Instance,
            new FakeEnvironment { EnvironmentName = Environments.Production });

        var context = CreateExceptionContext(new InvalidOperationException("Sensitive details"));

        filter.OnException(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var details = Assert.IsType<ProblemDetails>(result.Value);

        Assert.True(context.ExceptionHandled);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        Assert.Equal("Unexpected error", details.Title);
        Assert.Equal("An unexpected error occurred.", details.Detail);
    }

    private static AuthorizationFilterContext CreateAuthorizationContext()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, []);
    }

    private static ExceptionContext CreateExceptionContext(Exception exception)
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new ExceptionContext(actionContext, [])
        {
            Exception = exception
        };
    }

    private sealed class FakeEnvironment : IHostEnvironment, Microsoft.AspNetCore.Hosting.IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "api-aggregations-tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
        public string WebRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = null!;
    }
}
