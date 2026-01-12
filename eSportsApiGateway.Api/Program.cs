using eSportsApiGateway.Services;
using Scalar.AspNetCore;
using Sitech.Common;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var AllowSpecificOrigins = "AllowSpecificOrigins";


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowSpecificOrigins, policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyOrigin();
        policy.AllowAnyMethod();
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
app.MapOpenApi();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapScalarApiReference(options =>
{
    options.AddServer("https://pricing.sitechesports.com", "production");
});
app.Run();
