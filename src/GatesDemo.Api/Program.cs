// =============================================================================
// Program.cs — GatesDemo.Api Entry Point
// =============================================================================
// This file uses the .NET 8 minimal API pattern (top-level statements).
// All services, middleware, and endpoints are configured here in a single file
// for clarity and portfolio readability.
//
// Security features demonstrated:
//   - Options pattern for type-safe configuration binding
//   - Fixed-window rate limiting to prevent abuse
//   - CORS restricted to configured origins (not wildcard)
//   - HSTS enforcement in non-development environments
//   - HTTPS redirection
//   - Kestrel request size limits to prevent payload abuse
//   - ProblemDetails for RFC 7807 compliant error responses
//   - Strict redirect allowlist to prevent open redirect vulnerabilities
// =============================================================================

using System.Text.RegularExpressions;
using System.Threading.RateLimiting;
using GatesDemo.Api.Options;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// Service Registration
// =============================================================================

// Bind configuration sections to strongly-typed Options classes.
// This uses the Options pattern (IOptions<T>) so configuration values
// are injected via DI rather than read directly from IConfiguration.
// Each Options class defines its own SectionName constant matching the
// corresponding key in appsettings.json.
builder.Services.Configure<RedirectOptions>(
    builder.Configuration.GetSection(RedirectOptions.SectionName));
builder.Services.Configure<RateLimitOptions>(
    builder.Configuration.GetSection(RateLimitOptions.SectionName));
builder.Services.Configure<CorsOptions>(
    builder.Configuration.GetSection(CorsOptions.SectionName));

// Register ProblemDetails service for consistent RFC 7807 error responses.
// This ensures all error responses (400, 500, etc.) follow a standard JSON
// format with type, title, status, and detail fields.
builder.Services.AddProblemDetails();

// --- Rate Limiting ---
// Read rate limit config eagerly here (at startup) because AddRateLimiter
// needs the values during service registration, not at request time.
// Fallback to defaults if the config section is missing.
var rateLimitConfig = builder.Configuration
    .GetSection(RateLimitOptions.SectionName)
    .Get<RateLimitOptions>() ?? new RateLimitOptions();

// Fixed-window rate limiter: allows N requests per time window per client.
// Named policy "fixed" is applied per-endpoint via .RequireRateLimiting().
// This prevents abuse without requiring authentication.
builder.Services.AddRateLimiter(options =>
{
    // Return 429 Too Many Requests when limit is exceeded
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = rateLimitConfig.PermitLimit;       // Max requests per window
        opt.Window = TimeSpan.FromSeconds(rateLimitConfig.WindowSeconds); // Window duration
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;     // FIFO if queuing
        opt.QueueLimit = rateLimitConfig.QueueLimit;         // 0 = no queuing, reject immediately
    });
});

// --- CORS ---
// Read CORS config eagerly for the same reason as rate limiting.
// Origins are restricted to what's defined in appsettings.json.
// This is NOT a wildcard (*) policy — only explicitly listed origins are allowed.
var corsConfig = builder.Configuration
    .GetSection(CorsOptions.SectionName)
    .Get<CorsOptions>() ?? new CorsOptions();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // Only allow requests from configured origins
        policy.WithOrigins(corsConfig.AllowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register health checks — provides a simple /health endpoint for
// load balancers, container orchestrators, and monitoring tools.
builder.Services.AddHealthChecks();

// --- Kestrel Limits ---
// Set reasonable request limits to prevent oversized payload attacks.
// These are defense-in-depth measures on top of endpoint-level validation.
builder.WebHost.ConfigureKestrel(kestrel =>
{
    kestrel.Limits.MaxRequestBodySize = 1_048_576; // 1 MB max request body
    kestrel.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30); // Prevent slowloris-style attacks
});

var app = builder.Build();

// =============================================================================
// Middleware Pipeline
// =============================================================================
// Order matters here. Each middleware processes the request in sequence.

// Global exception handler — catches unhandled exceptions and returns
// a ProblemDetails response instead of leaking stack traces.
app.UseExceptionHandler();

// HSTS (HTTP Strict Transport Security) — tells browsers to only use HTTPS.
// Only enabled in non-development environments to avoid certificate issues locally.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Redirect HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Apply CORS policy (must be before endpoint routing).
app.UseCors();

// Apply rate limiting to endpoints that opt in via .RequireRateLimiting().
app.UseRateLimiter();

// =============================================================================
// Endpoint Definitions
// =============================================================================

// GET /health — Standard health check endpoint.
// No authentication required. Used by load balancers and monitoring.
// Returns 200 OK with "Healthy" text when the app is running.
app.MapHealthChecks("/health");

// GET /api/ping — Simple liveness check returning JSON.
// Useful for quick API verification and demo purposes.
app.MapGet("/api/ping", () => Results.Ok(new { status = "ok" }))
    .RequireRateLimiting("fixed");

// POST /api/echo — Accepts a JSON body with a "message" field,
// validates it, sanitizes whitespace, and returns the cleaned message.
// Demonstrates input validation and sanitization patterns.
app.MapPost("/api/echo", (EchoRequest request) =>
{
    // Validate: message is required
    if (string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.Problem(
            detail: "The message field is required.",
            statusCode: StatusCodes.Status400BadRequest);
    }

    // Sanitize: trim leading/trailing whitespace, collapse internal whitespace
    // to single spaces. This prevents whitespace abuse and normalizes input.
    var sanitized = Regex.Replace(request.Message.Trim(), @"\s+", " ");

    // Validate: enforce maximum length after sanitization
    if (sanitized.Length > 500)
    {
        return Results.Problem(
            detail: "Message exceeds the maximum length of 500 characters.",
            statusCode: StatusCodes.Status400BadRequest);
    }

    return Results.Ok(new { echo = sanitized });
})
.RequireRateLimiting("fixed");

// GET /api/redirect?target=... — Redirects to the target URL, but ONLY if
// the target host is in the configured allowlist.
//
// SECURITY: This is the key endpoint for demonstrating secure redirect patterns.
// Without the allowlist check, this would be an open redirect vulnerability
// (CWE-601) — which is exactly what the demo/vulnerable-codeql branch removes
// to trigger a CodeQL alert.
//
// Validation chain:
//   1. Target parameter must be present
//   2. Must be a valid absolute URI
//   3. Must use HTTPS scheme (no HTTP, no javascript:, no data:)
//   4. Host must be in the configured AllowedHosts list
//   5. Error messages never reflect raw user input (prevents XSS in error pages)
app.MapGet("/api/redirect", (string? target, IOptions<RedirectOptions> redirectOpts) =>
{
    if (string.IsNullOrWhiteSpace(target))
    {
        return Results.Problem(
            detail: "The target query parameter is required.",
            statusCode: StatusCodes.Status400BadRequest);
    }

    // Parse as absolute URI — rejects relative paths and malformed URLs
    if (!Uri.TryCreate(target, UriKind.Absolute, out var uri))
    {
        return Results.Problem(
            detail: "The target must be a valid absolute URL.",
            statusCode: StatusCodes.Status400BadRequest);
    }

    // Enforce HTTPS only — prevents redirect to javascript:, data:, ftp:, etc.
    if (!string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
    {
        return Results.Problem(
            detail: "Only HTTPS URLs are allowed.",
            statusCode: StatusCodes.Status400BadRequest);
    }

    // Check host against allowlist — the core defense against open redirect.
    // Uses case-insensitive comparison for domain names.
    var allowedHosts = redirectOpts.Value.AllowedHosts;

    if (!allowedHosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase))
    {
        // Note: we do NOT include the rejected host in the error message
        // to avoid reflecting user input (XSS defense-in-depth).
        return Results.Problem(
            detail: "The specified host is not in the allowlist.",
            statusCode: StatusCodes.Status400BadRequest);
    }

    // Safe to redirect — use uri.ToString() instead of raw target
    // to ensure the URL is properly normalized.
    return Results.Redirect(uri.ToString());
})
.RequireRateLimiting("fixed");

// =============================================================================
// DEMO VULNERABLE ENDPOINTS — Do not merge into main
// =============================================================================
// These endpoints intentionally contain security vulnerabilities to demonstrate
// CodeQL's ability to detect them. They exist only on the demo/vulnerable-codeql
// branch and should NEVER be merged.

// DEMO VULNERABLE (CWE-79): Reflected XSS — user input in HTML response
// CodeQL query: cs/web/xss
app.MapGet("/api/demo/xss", async (HttpContext ctx) =>
{
    var query = ctx.Request.Query["q"].ToString();
    if (string.IsNullOrWhiteSpace(query))
    {
        ctx.Response.StatusCode = 400;
        await ctx.Response.WriteAsync("Missing 'q' parameter");
        return;
    }

    // BAD: user input written directly into HTML without encoding
    ctx.Response.ContentType = "text/html";
    await ctx.Response.WriteAsync($"<h1>Search results for: {query}</h1>");
});

// DEMO VULNERABLE (CWE-601): Unvalidated redirect
// CodeQL query: cs/web/unvalidated-url-redirection
app.MapGet("/api/demo/open-redirect", (HttpContext ctx) =>
{
    var target = ctx.Request.Query["target"].ToString();
    if (string.IsNullOrWhiteSpace(target))
    {
        return Results.BadRequest(new { error = "target is required" });
    }

    // BAD: user input used directly in redirect without validation
    ctx.Response.Redirect(target);
    return Results.Empty;
})
.WithName("DemoOpenRedirect")
.WithTags("Demo");

app.Run();

// =============================================================================
// Request Models
// =============================================================================

// Record type for the /api/echo endpoint's JSON body.
// Using a record gives us immutability and value-based equality.
// Message is nullable because we validate it manually with clear error messages
// rather than relying on framework-level [Required] which gives less control.
public sealed record EchoRequest(string? Message);

// =============================================================================
// Partial Program Class
// =============================================================================
// This partial class declaration is required for integration testing.
// WebApplicationFactory<Program> needs to discover the entry point assembly,
// and minimal API projects don't have a Program class by default.
// Without this, the test project cannot reference the Program type.
public partial class Program;
