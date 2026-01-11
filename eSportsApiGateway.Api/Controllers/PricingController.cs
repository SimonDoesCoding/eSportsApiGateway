using eSportsApiGateway.Services;
using eSportsSimulator.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace eSportsApiGateway.Api.Controllers;

[ApiController]
[Route("")]
public class PricingController : ControllerBase
{
    private readonly ILogger<PricingController> _logger;
    private readonly ActivitySource _activitySource;
    private readonly IWebCall _webCall;

    public PricingController(ILogger<PricingController> logger, ActivitySource activitySource, IWebCall webCall)
    {
        _logger = logger;
        _activitySource = activitySource;
        _webCall = webCall;
    }

    [HttpPost]
    public async Task<IActionResult> GetPricing([FromBody]PriceRequest priceRequest)
    {
        using var activity = _activitySource.StartActivity("PricingController-GetPricing");
        var resp = await _webCall.PostAsync<object>("pricing", priceRequest);

        return Ok(resp);
    }
}

public record PriceResponse(
    double Probability,
    double TruePrice
);

public record PriceRequest(
    int SportId,
    string FixtureId,
    List<BetBuilderLeg> Legs
);