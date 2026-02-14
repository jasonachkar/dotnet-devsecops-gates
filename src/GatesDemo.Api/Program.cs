using System.Text.RegularExpressions;
using System.Threading.RateLimiting;
using GatesDemo.Api.Options;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// --- Options binding ---
builder.Services.Configure<RedirectOptions>(
    builder.Configuration.GetSection(RedirectOptions.SectionName));
builder.Services.Configure<RateLimitOptions>(
    builder.Configuration.GetSection(RateLimitOptions.SectionName));
builder.Services.Configure<CorsOptions>(
    builder.Configuration.GetSection(CorsOptions.SectionName));

// --- ProblemDetails ---
builder.Services.AddProblemDetails();

// --- Rate Limiting ---
var rateLimitConfig = builder.Configuration
    .GetSection(RateLimitOptions.SectionName)
    .Get<RateLimitOptions>() ?? new RateLimitOptions();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = rateLimitConfig.PermitLimit;
        opt.Window = TimeSpan.FromSeconds(rateLimitConfig.WindowSeconds);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = rateLimitConfig.QueueLimit;
    });
});

// --- CORS ---
var corsConfig = builder.Configuration
    .GetSection(CorsOptions.SectionName)
    .Get<CorsOptions>() ?? new CorsOptions();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsConfig.AllowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// --- Health checks ---
builder.Services.AddHealthChecks();

// --- Kestrel limits ---
builder.WebHost.ConfigureKestrel(kestrel =>
{
    kestrel.Limits.MaxRequestBodySize = 1_048_576; // 1 MB
    kestrel.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// --- Middleware pipeline ---
app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();

// --- Endpoints ---
app.MapHealthChecks("/health");

app.MapGet("/api/ping", () => Results.Ok(new { status = "ok" }))
    .RequireRateLimiting("fixed");

app.MapPost("/api/echo", (EchoRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.Problem(
            detail: "The message field is required.",
            statusCode: StatusCodes.Status400BadRequest);
    }

    var sanitized = Regex.Replace(request.Message.Trim(), @"\s+", " ");

    if (sanitized.Length > 500)
    {
        return Results.Problem(
            detail: "Message exceeds the maximum length of 500 characters.",
            statusCode: StatusCodes.Status400BadRequest);
    }

    return Results.Ok(new { echo = sanitized });
})
.RequireRateLimiting("fixed");

app.MapGet("/api/redirect", (string? target, IOptions<RedirectOptions> redirectOpts) =>
{
    if (string.IsNullOrWhiteSpace(target))
    {
        return Results.Problem(
            detail: "The target query parameter is required.",
            statusCode: StatusCodes.Status400BadRequest);
    }

    if (!Uri.TryCreate(target, UriKind.Absolute, out var uri))
    {
        return Results.Problem(
            detail: "The target must be a valid absolute URL.",
            statusCode: StatusCodes.Status400BadRequest);
    }

    if (!string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
    {
        return Results.Problem(
            detail: "Only HTTPS URLs are allowed.",
            statusCode: StatusCodes.Status400BadRequest);
    }

    var allowedHosts = redirectOpts.Value.AllowedHosts;

    if (!allowedHosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase))
    {
        return Results.Problem(
            detail: "The specified host is not in the allowlist.",
            statusCode: StatusCodes.Status400BadRequest);
    }

    return Results.Redirect(uri.ToString());
})
.RequireRateLimiting("fixed");

app.Run();

// --- Request models ---
public sealed record EchoRequest(string? Message);

// Required for WebApplicationFactory<Program> in integration tests
public partial class Program;
