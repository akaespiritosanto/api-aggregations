namespace api_aggregations.Filters;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public sealed class ApiKeyAuthFilter : IAsyncAuthorizationFilter
{
    private const string ApiKeyHeaderName = "X-API-KEY";
    private readonly string? _expectedApiKey;

    public ApiKeyAuthFilter(IConfiguration configuration)
    {
        // If API_KEY is not configured, the filter is effectively disabled (so we don't break existing setups).
        var configured = configuration["API_KEY"];
        var fromEnv = Environment.GetEnvironmentVariable("API_KEY");

        _expectedApiKey = (configured ?? fromEnv)?.Trim().Trim('"');
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (string.IsNullOrWhiteSpace(_expectedApiKey))
        {
            return Task.CompletedTask;
        }

        var providedApiKey = GetApiKeyFromRequest(context.HttpContext.Request);
        if (providedApiKey == _expectedApiKey)
        {
            return Task.CompletedTask;
        }

        context.Result = new UnauthorizedObjectResult(new ProblemDetails
        {
            Title = "Unauthorized",
            Detail = $"Missing or invalid API key. Send header '{ApiKeyHeaderName}'.",
            Status = StatusCodes.Status401Unauthorized
        });

        return Task.CompletedTask;
    }

    private static string? GetApiKeyFromRequest(HttpRequest request)
    {
        // 1) Preferred: X-API-KEY
        if (request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeader) &&
            apiKeyHeader.Count > 0)
        {
            return apiKeyHeader[0];
        }

        // 2) Also accept: Authorization: Bearer <key>
        if (request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            var value = authorizationHeader.ToString();
            const string prefix = "Bearer ";

            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return value[prefix.Length..].Trim();
            }
        }

        return null;
    }
}
