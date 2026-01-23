using eSportsApiGateway.Services;
using eSportsApiGateway.Api.Models;
using eSportsSimulator.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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

    /// <summary>
    /// Calculate pricing and probability for eSports betting scenarios
    /// </summary>
    /// <param name="priceRequest">The pricing request containing sport ID, fixture ID, and bet legs</param>
    /// <returns>Pricing response with probability and true price</returns>
    /// <response code="200">Returns the calculated pricing information</response>
    /// <response code="400">Invalid request format or missing required fields</response>
    /// <response code="401">Invalid or missing API key</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(PriceResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetPricing([FromBody]PriceRequest priceRequest)
    {
        using var activity = _activitySource.StartActivity("PricingController-GetPricing");
        
        // Get client info from middleware
        var clientName = HttpContext.Items["ClientName"]?.ToString();
        activity?.SetTag("client.name", clientName);
        
        _logger.LogInformation("Processing pricing request for client: {ClientName}, SportId: {SportId}, FixtureId: {FixtureId}", 
            clientName, priceRequest.SportId, priceRequest.FixtureId);

        try
        {
            var resp = await _webCall.PostAsync<PriceResponse>("pricing", priceRequest);
            
            _logger.LogInformation("Pricing request completed successfully for client: {ClientName}", clientName);
            return Ok(resp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing pricing request for client: {ClientName}", clientName);
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while processing your request" });
        }
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