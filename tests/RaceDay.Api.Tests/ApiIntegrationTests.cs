using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using Xunit;

namespace RaceDay.Api.Tests;
public class ApiIntegrationTests : IClassFixture<RaceDayApiFactory>
{
    private readonly RaceDayApiFactory _factory;
    public ApiIntegrationTests(RaceDayApiFactory factory) => _factory = factory;

    private async Task<string> RegisterAndLogin(string email, string role)
    {
        var client = _factory.CreateClient();
        var register = await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test", lastName = role, email, password = "StrongPass!123", role
        });
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "StrongPass!123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var data = await login.Content.ReadFromJsonAsync<AuthResult>();
        Assert.False(string.IsNullOrWhiteSpace(data!.Token));
        return data.Token;
    }

    [Fact]
    public async Task RegistrationAndLoginReturnTokenWithoutReturningPasswordHash()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Ada", lastName = "Runner", email = $"ada{Guid.NewGuid():N}@example.test",
            password = "StrongPass!123", role = "Participant"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("PasswordHash", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ParticipantIsForbiddenFromCreatingAnEvent()
    {
        var token = await RegisterAndLogin($"runner{Guid.NewGuid():N}@example.test", "Participant");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.PostAsJsonAsync("/api/events", new
        {
            title = "Test Run", description = "Integration test", location = "Johannesburg",
            eventDate = DateTime.UtcNow.AddDays(14), eventType = "Run"
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task OrganiserCanCreateAnEventAndPublicCanViewIt()
    {
        var token = await RegisterAndLogin($"organiser{Guid.NewGuid():N}@example.test", "Organiser");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var created = await client.PostAsJsonAsync("/api/events", new
        {
            title = "CI Test Run", description = "Created by test", location = "Johannesburg",
            eventDate = DateTime.UtcNow.AddDays(14), eventType = "Run"
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        client.DefaultRequestHeaders.Authorization = null;
        var list = await client.GetAsync("/api/events");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.Contains("CI Test Run", await list.Content.ReadAsStringAsync());
    }

    private sealed record AuthResult(string Token);
}
