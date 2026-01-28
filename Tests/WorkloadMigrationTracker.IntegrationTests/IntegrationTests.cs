using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace WorkloadMigrationTracker.IntegrationTests;

public class HealthSmokeTests : IAsyncLifetime
{
    private HttpClient _client = new HttpClient();
    private string _baseUrl;

    public Task InitializeAsync()
    {
        _baseUrl = Environment.GetEnvironmentVariable("TEST_API_BASE_URL") ?? Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? "http://localhost:5000";
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ApiHealth_ReturnsHealthy()
    {
        var res = await _client.GetAsync(_baseUrl + "/api/health");
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("Healthy", doc.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task DbHealth_ReturnsConnected()
    {
        var res = await _client.GetAsync(_baseUrl + "/api/health/database");
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("Healthy", doc.RootElement.GetProperty("status").GetString());
        Assert.Equal("Connected", doc.RootElement.GetProperty("database").GetString());
    }
}
