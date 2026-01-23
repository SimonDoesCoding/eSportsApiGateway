using eSportsApiGateway.Api.Models;
using Microsoft.Extensions.Options;
using System.Net;

namespace eSportsApiGateway.Api.Middleware;

public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiKeySettings _apiKeySettings;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;
    
    private const string ApiKeyHeaderName = "X-API-Key";
    private const string ApiKeyQueryParam = "apikey"; // Fallback option

    public ApiKeyAuthenticationMiddleware(
        RequestDelegate next, 
        IOptions<ApiKeySettings> apiKeySettings,
        ILogger<ApiKeyAuthenticationMiddleware> logger)
    {
        _next = next;
        _apiKeySettings = apiKeySettings.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authentication for OpenAPI/Swagger endpoints
        if (IsSwaggerEndpoint(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var apiKey = ExtractApiKey(context.Request);
        
        if (string.IsNullOrEmpty(apiKey))
        {
            await HandleUnauthorized(context, "API key is required");
            return;
        }

        var apiKeyConfig = ValidateApiKey(apiKey);
        if (apiKeyConfig == null)
        {
            await HandleUnauthorized(context, "Invalid API key");
            return;
        }

        if (!apiKeyConfig.IsActive)
        {
            await HandleUnauthorized(context, "API key is inactive");
            return;
        }

        if (apiKeyConfig.ExpiresAt.HasValue && apiKeyConfig.ExpiresAt.Value < DateTime.UtcNow)
        {
            await HandleUnauthorized(context, "API key has expired");
            return;
        }

        // Add client info to context for potential use in controllers
        context.Items["ApiKeyConfig"] = apiKeyConfig;
        context.Items["ClientName"] = apiKeyConfig.Name;

        _logger.LogInformation("API request authenticated for client: {ClientName}", apiKeyConfig.Name);

        await _next(context);
    }

    private string? ExtractApiKey(HttpRequest request)
    {
        // Try header first (recommended)
        if (request.Headers.TryGetValue(ApiKeyHeaderName, out var headerValue))
        {
            return headerValue.FirstOrDefault();
        }

        // Fallback to query parameter (less secure)
        if (request.Query.TryGetValue(ApiKeyQueryParam, out var queryValue))
        {
            return queryValue.FirstOrDefault();
        }

        return null;
    }

    private ApiKeyConfig? ValidateApiKey(string apiKey)
    {
        return _apiKeySettings.ApiKeys.TryGetValue(apiKey, out var config) ? config : null;
    }

    private static bool IsSwaggerEndpoint(PathString path)
    {
        return path.StartsWithSegments("/openapi") || 
               path.StartsWithSegments("/scalar") ||
               path.StartsWithSegments("/swagger");
    }

    private static async Task HandleUnauthorized(HttpContext context, string message)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentType = "application/json";
        
        var response = new { error = "Unauthorized", message };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
}