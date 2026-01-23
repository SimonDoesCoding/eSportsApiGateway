namespace eSportsApiGateway.Api.Models;

public class ApiKeySettings
{
    public Dictionary<string, ApiKeyConfig> ApiKeys { get; set; } = new();
}

public class ApiKeyConfig
{
    public string Name { get; set; } = string.Empty;
    public List<string> AllowedOrigins { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
    public string? Description { get; set; }
}

public class CorsSettings
{
    public List<string> Development { get; set; } = new();
    public List<string> Production { get; set; } = new();
    public List<string> Staging { get; set; } = new();
}