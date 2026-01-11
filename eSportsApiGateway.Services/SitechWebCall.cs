using eSportsSimulator.DTO;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace eSportsApiGateway.Services;

public record BetBuilderRequest(int SportId, string FixtureId, List<BetBuilderLeg> Legs);

public class SitechWebCall : IWebCall
{
    private readonly ActivitySource _activitySource;
    private readonly HttpClient _httpClient;

    public SitechWebCall(ActivitySource activitySource, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _activitySource = activitySource;
    }

    public async Task<TReturn?> PostAsync<TReturn>(string url, object body)
    {
        var resp = await _httpClient.PostAsJsonAsync(url, body);
        var respBody = await resp.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<TReturn>(respBody) ?? default;
    }
}

public interface IWebCall
{
    Task<TReturn> PostAsync<TReturn>(string url, object body);
}
