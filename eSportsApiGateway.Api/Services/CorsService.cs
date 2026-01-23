using eSportsApiGateway.Api.Models;
using Microsoft.Extensions.Options;

namespace eSportsApiGateway.Api.Services;

public interface ICorsService
{
    bool IsOriginAllowed(string origin, string? apiKey = null);
    List<string> GetAllowedOrigins(string? apiKey = null);
}

public class CorsService : ICorsService
{
    private readonly ApiKeySettings _apiKeySettings;
    private readonly CorsSettings _corsSettings;
    private readonly IWebHostEnvironment _environment;

    public CorsService(
        IOptions<ApiKeySettings> apiKeySettings,
        IOptions<CorsSettings> corsSettings,
        IWebHostEnvironment environment)
    {
        _apiKeySettings = apiKeySettings.Value;
        _corsSettings = corsSettings.Value;
        _environment = environment;
    }

    public bool IsOriginAllowed(string origin, string? apiKey = null)
    {
        var allowedOrigins = GetAllowedOrigins(apiKey);
        return allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
    }

    public List<string> GetAllowedOrigins(string? apiKey = null)
    {
        var origins = new List<string>();

        // Add environment-specific origins
        if (_environment.IsDevelopment())
        {
            origins.AddRange(_corsSettings.Development);
        }
        else if (_environment.IsProduction())
        {
            origins.AddRange(_corsSettings.Production);
        }
        else if (_environment.IsStaging())
        {
            origins.AddRange(_corsSettings.Staging);
        }

        // Add client-specific origins if API key is provided
        if (!string.IsNullOrEmpty(apiKey) && 
            _apiKeySettings.ApiKeys.TryGetValue(apiKey, out var config))
        {
            origins.AddRange(config.AllowedOrigins);
        }

        return origins.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }
}