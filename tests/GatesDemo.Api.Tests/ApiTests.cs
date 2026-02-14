// =============================================================================
// ApiTests.cs — Integration Tests for GatesDemo.Api
// =============================================================================
// These are integration tests using WebApplicationFactory, which spins up the
// entire ASP.NET Core pipeline in-memory (no real HTTP port needed).
// This approach tests the full request/response cycle including middleware,
// routing, model binding, and validation — not just isolated unit logic.
//
// Why WebApplicationFactory instead of unit tests?
//   - Tests the actual HTTP pipeline, not mocked abstractions
//   - Catches routing issues, middleware ordering bugs, and serialization problems
//   - Runs in-process via TestServer — fast, no port conflicts, works on Linux CI
//
// The test class implements IClassFixture<WebApplicationFactory<Program>> which
// shares a single server instance across all tests in this class for performance.
// =============================================================================

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GatesDemo.Api.Tests;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiTests(WebApplicationFactory<Program> factory)
    {
        // Create an HttpClient configured for the test server.
        // AllowAutoRedirect = false is CRITICAL for redirect tests:
        // without this, the client would automatically follow the 302
        // and we'd see a 200 from the target URL instead of the redirect itself.
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    // =========================================================================
    // Test: GET /api/ping returns 200 OK with { "status": "ok" }
    // =========================================================================
    // Verifies the simplest endpoint works and returns the expected JSON shape.
    // This is the baseline "is the API alive?" test.
    [Fact]
    public async Task Ping_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/ping");

        // Should return 200 OK
        response.EnsureSuccessStatusCode();

        // Deserialize and verify the JSON body matches expected shape
        var body = await response.Content.ReadFromJsonAsync<PingResponse>();
        Assert.NotNull(body);
        Assert.Equal("ok", body.Status);
    }

    // =========================================================================
    // Test: GET /api/redirect rejects hosts NOT in the allowlist
    // =========================================================================
    // This is the core security test. It verifies that the redirect endpoint
    // rejects URLs with hosts that aren't in appsettings.json AllowedHosts.
    // "evil.com" is not in the allowlist, so it should return 400 Bad Request.
    //
    // If this test passes, the open redirect vulnerability is mitigated.
    // The demo/vulnerable-codeql branch would FAIL this test because it
    // removes the allowlist check.
    [Fact]
    public async Task Redirect_RejectsNonAllowlistedHost()
    {
        var response = await _client.GetAsync("/api/redirect?target=https://evil.com/path");

        // Should return 400 Bad Request — host is not allowlisted
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // =========================================================================
    // Test: GET /api/redirect allows hosts that ARE in the allowlist
    // =========================================================================
    // Verifies that "example.com" (which IS in appsettings.json AllowedHosts)
    // results in a proper 302 redirect with the correct Location header.
    //
    // Note: AllowAutoRedirect = false in the client setup ensures we see
    // the 302 response directly instead of following it.
    [Fact]
    public async Task Redirect_AllowsAllowlistedHost()
    {
        var response = await _client.GetAsync("/api/redirect?target=https://example.com/page");

        // Should return 302 Found (redirect)
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        // The Location header should contain the exact URL we requested
        Assert.Equal("https://example.com/page", response.Headers.Location?.ToString());
    }

    // Record type for deserializing the /api/ping JSON response.
    // System.Text.Json will map { "status": "ok" } to PingResponse.Status.
    private sealed record PingResponse(string Status);
}
