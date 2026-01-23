using eSportsApiGateway.Services;
using eSportsApiGateway.Api.Models;
using eSportsApiGateway.Api.Middleware;
using eSportsApiGateway.Api.Services;
using Scalar.AspNetCore;
using Sitech.Common;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var AllowSpecificOrigins = "AllowSpecificOrigins";

// Configure settings
builder.Services.Configure<ApiKeySettings>(
    builder.Configuration.GetSection("ApiKeySettings"));
builder.Services.Configure<CorsSettings>(
    builder.Configuration.GetSection("CorsSettings"));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register services
builder.Services.AddSingleton<ICorsService, CorsService>();

// Configure CORS with dynamic origin validation
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowSpecificOrigins, policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            var corsService = builder.Services.BuildServiceProvider()
                .GetRequiredService<ICorsService>();
            return corsService.IsOriginAllowed(origin);
        });
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowCredentials();
    });
});

builder.Services.RegisterOtel();
builder.Services.AddSingleton<IWebCall, SitechWebCall>();
builder.Services.AddSingleton(x =>
{
    var cfg = builder.Configuration.GetRequiredSection("SitecheSportsConfig");
    var baseUrl = cfg.GetValue<string>("BaseUrl") ?? throw new MissingFieldException("Sitech Base Url is missing from the config");

    return new HttpClient
    {
        BaseAddress = new Uri(baseUrl),
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors(AllowSpecificOrigins);

// Add API key authentication middleware
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

app.MapOpenApi();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapScalarApiReference(options =>
{
    options.AddServer("https://pricing.sitechesports.com", "production");
});
app.Run();
