using System.Net;
using UserManagement.Tests.Infrastructure;
using Xunit.Abstractions;

namespace UserManagement.Tests.Tests.E2E;

public class EndToEndTests : UsersApiTestBase
{
    public EndToEndTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    [Trait("TestCase", "TC-E2E-01")]
    public async Task UserLifecycle_CreateGetUpdateGet_ReturnsConsistentData()
    {
        var createResponse = await PostUserAsync(new { countryIsoCode = "ru", locale = "ru-RU" });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await ReadUserAsync(createResponse);
        Assert.NotEqual(Guid.Empty, created.UserId);
        Assert.Equal("RU", created.CountryIsoCode);
        Assert.Equal("ru-RU", created.Locale);
        Assert.Equal(UtcNow, created.DateCreated);
        Assert.Null(created.DateModified);

        var getResponse = await GetUserAsync(created.UserId);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(created, await ReadUserAsync(getResponse));

        Factory.FakeTimeProvider.Advance(TimeSpan.FromMinutes(5));
        var updateResponse = await PutUserAsync(created.UserId, new { countryIsoCode = "us", locale = "en-US" });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await ReadUserAsync(updateResponse);
        Assert.Equal(created.UserId, updated.UserId);
        Assert.Equal("US", updated.CountryIsoCode);
        Assert.Equal("en-US", updated.Locale);
        Assert.Equal(created.DateCreated, updated.DateCreated);
        Assert.Equal(UtcNow, updated.DateModified);

        var getAfterUpdateResponse = await GetUserAsync(created.UserId);

        Assert.Equal(HttpStatusCode.OK, getAfterUpdateResponse.StatusCode);
        Assert.Equal(updated, await ReadUserAsync(getAfterUpdateResponse));
    }

    [Fact]
    [Trait("TestCase", "TC-E2E-02")]
    public async Task MultipleUsers_CreateListUpdateOne_OnlyUpdatedUserChanges()
    {
        var first = await CreateUserAsync("RU", "ru-RU");
        var second = await CreateUserAsync("DE", "de-DE");
        var third = await CreateUserAsync("FR", "fr-FR");

        var usersBeforeUpdate = await GetAllUsersAsync();

        Assert.Equal(3, usersBeforeUpdate.Count);
        Assert.Contains(first, usersBeforeUpdate);
        Assert.Contains(second, usersBeforeUpdate);
        Assert.Contains(third, usersBeforeUpdate);

        Factory.FakeTimeProvider.Advance(TimeSpan.FromMinutes(1));
        var updateResponse = await PutUserAsync(second.UserId, new { countryIsoCode = "US", locale = "en-US" });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updatedSecond = await ReadUserAsync(updateResponse);

        var usersAfterUpdate = await GetAllUsersAsync();

        Assert.Equal(3, usersAfterUpdate.Count);
        Assert.Contains(first, usersAfterUpdate);
        Assert.Contains(updatedSecond, usersAfterUpdate);
        Assert.Contains(third, usersAfterUpdate);
        Assert.DoesNotContain(second, usersAfterUpdate);
    }
}
