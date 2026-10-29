using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.API.Interfaces;
using UserManagement.Tests.Infrastructure;
using Xunit.Abstractions;

namespace UserManagement.Tests.Tests.Integration;

public class ExceptionHandlingTests : UsersApiTestBase
{
    private readonly HttpClient _failingClient;

    public ExceptionHandlingTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _failingClient = Factory
            .WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
                services.AddSingleton<IUserRepository, ThrowingUserRepository>()))
            .CreateClient();
    }

    private static HttpRequestMessage CreateRequest(string method, string url)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method is "POST" or "PUT")
            request.Content = JsonContent.Create(new { countryIsoCode = "RU", locale = "ru-RU" });

        return request;
    }

    [Theory]
    [Trait("TestCase", "TC-ERR-01")]
    [Trait("TestCase", "TC-ERR-02")]
    [Trait("TestCase", "TC-ERR-03")]
    [InlineData("GET", UsersUrl)]
    [InlineData("GET", UsersUrl + "/3fa85f64-5717-4562-b3fc-2c963f66afa6")]
    [InlineData("POST", UsersUrl)]
    [InlineData("PUT", UsersUrl + "/3fa85f64-5717-4562-b3fc-2c963f66afa6")]
    public async Task AnyEndpoint_UnexpectedException_ReturnsCleanJsonError(string method, string url)
    {
        var response = await _failingClient.SendAsync(CreateRequest(method, url));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        Assert.Equal(500, document.RootElement.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(document.RootElement.GetProperty("title").GetString()));

        Assert.DoesNotContain(ThrowingUserRepository.ErrorMessage, json);
        Assert.DoesNotContain(nameof(InvalidOperationException), json);
        Assert.DoesNotContain(nameof(ThrowingUserRepository), json);
    }

    [Fact]
    [Trait("TestCase", "TC-ERR-04")]
    public async Task UnexpectedException_ServerKeepsHandlingRequests()
    {
        var first = await _failingClient.GetAsync(UsersUrl);
        var second = await _failingClient.GetAsync(UsersUrl);

        Assert.Equal(HttpStatusCode.InternalServerError, first.StatusCode);
        Assert.Equal(HttpStatusCode.InternalServerError, second.StatusCode);
        Assert.Equal("application/problem+json", second.Content.Headers.ContentType?.MediaType);
    }
}
