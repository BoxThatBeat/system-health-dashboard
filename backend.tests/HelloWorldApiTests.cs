using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;

namespace backend.tests;

public class HelloWorldApiTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task GetHello_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/hello");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetHello_ReturnsJsonContent()
    {
        // Act
        var response = await _client.GetAsync("/api/hello");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
        Assert.That(content, Is.Not.Null);
        Assert.That(content, Does.Contain("message"));
    }

    [Test]
    public async Task GetHello_ReturnsHelloWorldMessage()
    {
        // Act
        var response = await _client.GetAsync("/api/hello");
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);

        // Assert
        var message = jsonDocument.RootElement.GetProperty("message").GetString();
        Assert.That(message, Is.EqualTo("Hello World"));
    }

    [Test]
    public async Task GetHello_ReturnsTimestamp()
    {
        // Act
        var response = await _client.GetAsync("/api/hello");
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);

        // Assert
        Assert.That(jsonDocument.RootElement.TryGetProperty("timestamp", out var timestamp), Is.True);
        Assert.That(timestamp.ValueKind, Is.EqualTo(JsonValueKind.String));
    }
}
