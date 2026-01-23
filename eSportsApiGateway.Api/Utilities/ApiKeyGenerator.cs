using System.Security.Cryptography;
using System.Text;

namespace eSportsApiGateway.Api.Utilities;

public static class ApiKeyGenerator
{
    private const string DevPrefix = "ak_dev_";
    private const string StagingPrefix = "ak_staging_";
    private const string LivePrefix = "ak_live_";
    
    public enum Environment
    {
        Development,
        Staging,
        Production
    }

    /// <summary>
    /// Generates a cryptographically secure API key
    /// </summary>
    /// <param name="environment">The environment for which to generate the key</param>
    /// <param name="length">The length of the random portion (default: 32)</param>
    /// <returns>A formatted API key string</returns>
    public static string GenerateApiKey(Environment environment, int length = 32)
    {
        if (length < 16)
            throw new ArgumentException("API key length must be at least 16 characters", nameof(length));

        var prefix = environment switch
        {
            Environment.Development => DevPrefix,
            Environment.Staging => StagingPrefix,
            Environment.Production => LivePrefix,
            _ => throw new ArgumentException("Invalid environment", nameof(environment))
        };

        var randomBytes = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        var randomString = Convert.ToHexString(randomBytes).ToLowerInvariant();
        return $"{prefix}{randomString}";
    }

    /// <summary>
    /// Validates the format of an API key
    /// </summary>
    /// <param name="apiKey">The API key to validate</param>
    /// <returns>True if the format is valid</returns>
    public static bool IsValidFormat(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
            return false;

        return apiKey.StartsWith(DevPrefix) || 
               apiKey.StartsWith(StagingPrefix) || 
               apiKey.StartsWith(LivePrefix);
    }

    /// <summary>
    /// Extracts the environment from an API key
    /// </summary>
    /// <param name="apiKey">The API key</param>
    /// <returns>The environment or null if invalid</returns>
    public static Environment? GetEnvironment(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
            return null;

        if (apiKey.StartsWith(DevPrefix))
            return Environment.Development;
        if (apiKey.StartsWith(StagingPrefix))
            return Environment.Staging;
        if (apiKey.StartsWith(LivePrefix))
            return Environment.Production;

        return null;
    }

    /// <summary>
    /// Generates a configuration template for a new client
    /// </summary>
    /// <param name="clientName">Name of the client</param>
    /// <param name="environment">Environment for the key</param>
    /// <param name="allowedOrigins">List of allowed origins</param>
    /// <param name="expirationMonths">Optional expiration in months</param>
    /// <returns>JSON configuration string</returns>
    public static string GenerateClientConfig(
        string clientName, 
        Environment environment, 
        List<string> allowedOrigins, 
        int? expirationMonths = null)
    {
        var apiKey = GenerateApiKey(environment);
        var config = new
        {
            ApiKey = apiKey,
            Config = new
            {
                Name = clientName,
                AllowedOrigins = allowedOrigins,
                IsActive = true,
                ExpiresAt = expirationMonths.HasValue 
                    ? DateTime.UtcNow.AddMonths(expirationMonths.Value).ToString("yyyy-MM-ddTHH:mm:ssZ")
                    : (string?)null,
                Description = $"API key for {clientName} - {environment} environment"
            }
        };

        return System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }
}