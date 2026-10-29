using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using UserManagement.Tests.Models;
using Xunit.Abstractions;

namespace UserManagement.Tests.Infrastructure;

public abstract class UsersApiTestBase : IDisposable
{
    protected const string UsersUrl = "/api/users";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected CustomWebApplicationFactory Factory { get; }
    protected HttpClient Client { get; }

    protected UsersApiTestBase(ITestOutputHelper testOutputHelper)
    {
        Factory = new CustomWebApplicationFactory(testOutputHelper);
        Client = Factory.CreateClient();
    }

    protected DateTime UtcNow => Factory.FakeTimeProvider.GetUtcNow().UtcDateTime;

    protected Task<HttpResponseMessage> PostUserAsync(object body)
        => Client.PostAsJsonAsync(UsersUrl, body);

    protected Task<HttpResponseMessage> PostRawJsonAsync(string json)
        => Client.PostAsync(UsersUrl, new StringContent(json, Encoding.UTF8, "application/json"));

    protected Task<HttpResponseMessage> PutUserAsync(Guid userId, object body)
        => PutUserAsync(userId.ToString(), body);

    protected Task<HttpResponseMessage> PutUserAsync(string userId, object body)
        => Client.PutAsJsonAsync($"{UsersUrl}/{userId}", body);

    protected Task<HttpResponseMessage> PutRawJsonAsync(Guid userId, string json)
        => Client.PutAsync($"{UsersUrl}/{userId}", new StringContent(json, Encoding.UTF8, "application/json"));

    protected Task<HttpResponseMessage> GetUserAsync(Guid userId)
        => GetUserAsync(userId.ToString());

    protected Task<HttpResponseMessage> GetUserAsync(string userId)
        => Client.GetAsync($"{UsersUrl}/{userId}");

    protected Task<HttpResponseMessage> GetUsersAsync(string query = "")
        => Client.GetAsync(UsersUrl + query);

    protected async Task<UserResponse> CreateUserAsync(string countryIsoCode = "RU", string locale = "ru-RU")
    {
        var response = await PostUserAsync(new { countryIsoCode, locale });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return await ReadUserAsync(response);
    }

    protected async Task<List<UserResponse>> GetAllUsersAsync()
    {
        var response = await GetUsersAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return await ReadUsersAsync(response);
    }

    protected static async Task<UserResponse> ReadUserAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);

        UserSchemaAssert.IsValidUser(document.RootElement);

        return document.RootElement.Deserialize<UserResponse>(JsonOptions)!;
    }

    protected static async Task<List<UserResponse>> ReadUsersAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);

        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
        foreach (var user in document.RootElement.EnumerateArray())
            UserSchemaAssert.IsValidUser(user);

        return document.RootElement.Deserialize<List<UserResponse>>(JsonOptions)!;
    }

    /// <summary>
    /// Asserts a 400 response in ValidationProblemDetails format
    /// with a non-empty error message for the given field.
    /// </summary>
    protected static async Task AssertValidationErrorAsync(HttpResponseMessage response, string field)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);

        Assert.True(
            document.RootElement.GetProperty("errors").TryGetProperty(field, out var messages),
            $"Expected a validation error for '{field}', but got: {json}");
        Assert.NotEmpty(messages.EnumerateArray().Select(m => m.GetString()).Where(m => !string.IsNullOrWhiteSpace(m)));
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
    }
}
