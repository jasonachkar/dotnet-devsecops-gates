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
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Ping_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/ping");

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<PingResponse>();
        Assert.NotNull(body);
        Assert.Equal("ok", body.Status);
    }

    [Fact]
    public async Task Redirect_RejectsNonAllowlistedHost()
    {
        var response = await _client.GetAsync("/api/redirect?target=https://evil.com/path");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Redirect_AllowsAllowlistedHost()
    {
        var response = await _client.GetAsync("/api/redirect?target=https://example.com/page");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://example.com/page", response.Headers.Location?.ToString());
    }

    private sealed record PingResponse(string Status);
}
