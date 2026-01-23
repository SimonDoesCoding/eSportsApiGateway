// Simple console application to generate API keys
// Usage: dotnet run GenerateApiKey.cs

using System.Security.Cryptography;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("eSports API Gateway - API Key Generator");
        Console.WriteLine("=====================================");
        
        if (args.Length > 0 && args[0] == "--help")
        {
            ShowHelp();
            return;
        }

        Console.Write("Enter client name: ");
        var clientName = Console.ReadLine();
        
        Console.Write("Enter environment (dev/staging/prod): ");
        var envInput = Console.ReadLine()?.ToLower();
        
        var environment = envInput switch
        {
            "dev" or "development" => "Development",
            "staging" => "Staging", 
            "prod" or "production" => "Production",
            _ => "Development"
        };

        Console.Write("Enter allowed origins (comma-separated): ");
        var originsInput = Console.ReadLine();
        var origins = originsInput?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(o => o.Trim()).ToList() ?? new List<string>();

        Console.Write("Expiration in months (optional, press Enter to skip): ");
        var expirationInput = Console.ReadLine();
        int? expirationMonths = null;
        if (int.TryParse(expirationInput, out var months))
        {
            expirationMonths = months;
        }

        var apiKey = GenerateApiKey(environment);
        
        Console.WriteLine("\n" + new string('=', 50));
        Console.WriteLine("GENERATED API KEY CONFIGURATION");
        Console.WriteLine(new string('=', 50));
        
        Console.WriteLine($"\nAPI Key: {apiKey}");
        Console.WriteLine($"Client: {clientName}");
        Console.WriteLine($"Environment: {environment}");
        
        Console.WriteLine("\nConfiguration to add to appsettings.json:");
        Console.WriteLine(new string('-', 40));
        
        var config = GenerateConfig(apiKey, clientName, origins, expirationMonths);
        Console.WriteLine(config);
        
        Console.WriteLine("\nIMPORTANT SECURITY NOTES:");
        Console.WriteLine("- Store this API key securely");
        Console.WriteLine("- Never commit API keys to source control");
        Console.WriteLine("- Provide the key to the client through secure channels");
        Console.WriteLine("- Monitor API key usage and rotate regularly");
    }

    static void ShowHelp()
    {
        Console.WriteLine("API Key Generator for eSports API Gateway");
        Console.WriteLine("\nThis tool generates secure API keys and configuration templates.");
        Console.WriteLine("\nUsage: dotnet run GenerateApiKey.cs");
        Console.WriteLine("\nThe tool will prompt for:");
        Console.WriteLine("- Client name");
        Console.WriteLine("- Environment (dev/staging/prod)");
        Console.WriteLine("- Allowed origins (comma-separated URLs)");
        Console.WriteLine("- Optional expiration period");
    }

    static string GenerateApiKey(string environment)
    {
        var prefix = environment switch
        {
            "Development" => "ak_dev_",
            "Staging" => "ak_staging_",
            "Production" => "ak_live_",
            _ => "ak_dev_"
        };

        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        var randomString = Convert.ToHexString(randomBytes).ToLowerInvariant();
        return $"{prefix}{randomString}";
    }

    static string GenerateConfig(string apiKey, string? clientName, List<string> origins, int? expirationMonths)
    {
        var expirationDate = expirationMonths.HasValue 
            ? DateTime.UtcNow.AddMonths(expirationMonths.Value).ToString("yyyy-MM-ddTHH:mm:ssZ")
            : null;

        var config = new
        {
            ApiKeys = new Dictionary<string, object>
            {
                [apiKey] = new
                {
                    Name = clientName ?? "New Client",
                    AllowedOrigins = origins,
                    IsActive = true,
                    ExpiresAt = expirationDate,
                    Description = $"API key for {clientName}"
                }
            }
        };

        return System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }
}