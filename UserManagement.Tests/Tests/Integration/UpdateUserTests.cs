using System.Net;
using UserManagement.Tests.Infrastructure;
using Xunit.Abstractions;

namespace UserManagement.Tests.Tests.Integration;

public class UpdateUserTests : UsersApiTestBase
{
    public UpdateUserTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    [Trait("TestCase", "TC-PUT-01")]
    [Trait("TestCase", "TC-PUT-02")]
    [Trait("TestCase", "TC-PUT-03")]
    [Trait("TestCase", "TC-PUT-04")]
    [Trait("TestCase", "TC-PUT-05")]
    public async Task UpdateUser_ExistingUser_ReturnsUpdatedUser()
    {
        var created = await CreateUserAsync("RU", "ru-RU");
        Factory.FakeTimeProvider.Advance(TimeSpan.FromMinutes(5));

        var response = await PutUserAsync(created.UserId, new { countryIsoCode = "US", locale = "en-US" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await ReadUserAsync(response);
        Assert.Equal(created.UserId, updated.UserId);
        Assert.Equal(created.DateCreated, updated.DateCreated);
        Assert.Equal("US", updated.CountryIsoCode);
        Assert.Equal("en-US", updated.Locale);
        Assert.Equal(UtcNow, updated.DateModified);
        Assert.True(updated.DateModified > updated.DateCreated);
    }

    [Theory]
    [Trait("TestCase", "TC-PUT-06")]
    [InlineData("us")]
    [InlineData("uS")]
    public async Task UpdateUser_WithLowercaseCountry_StoresUppercase(string countryIsoCode)
    {
        var created = await CreateUserAsync();

        var response = await PutUserAsync(created.UserId, new { countryIsoCode, locale = "en-US" });

        Assert.Equal("US", (await ReadUserAsync(response)).CountryIsoCode);
    }

    [Fact]
    [Trait("TestCase", "TC-PUT-07")]
    public async Task UpdateUser_Twice_MovesDateModifiedForward()
    {
        var created = await CreateUserAsync();
        Factory.FakeTimeProvider.Advance(TimeSpan.FromMinutes(1));
        var first = await ReadUserAsync(await PutUserAsync(created.UserId, new { countryIsoCode = "US", locale = "en-US" }));
        Factory.FakeTimeProvider.Advance(TimeSpan.FromMinutes(1));

        var second = await ReadUserAsync(await PutUserAsync(created.UserId, new { countryIsoCode = "DE", locale = "de-DE" }));

        Assert.Equal(UtcNow, second.DateModified);
        Assert.True(second.DateModified > first.DateModified);
    }

    [Fact]
    [Trait("TestCase", "TC-PUT-08")]
    public async Task UpdateUser_ThenGetById_ReturnsUpdatedData()
    {
        var created = await CreateUserAsync();
        var updated = await ReadUserAsync(await PutUserAsync(created.UserId, new { countryIsoCode = "US", locale = "en-US" }));

        var response = await GetUserAsync(created.UserId);

        Assert.Equal(updated, await ReadUserAsync(response));
    }

    [Fact]
    [Trait("TestCase", "TC-PUT-09")]
    public async Task UpdateUser_OneOfSeveral_DoesNotAffectOthers()
    {
        var target = await CreateUserAsync("RU", "ru-RU");
        var other = await CreateUserAsync("DE", "de-DE");

        await PutUserAsync(target.UserId, new { countryIsoCode = "US", locale = "en-US" });

        Assert.Equal(other, await ReadUserAsync(await GetUserAsync(other.UserId)));
    }

    [Fact]
    [Trait("TestCase", "TC-PUT-10")]
    [Trait("TestCase", "TC-PUT-11")]
    public async Task UpdateUser_NonExistentId_ReturnsNotFoundAndDoesNotCreateUser()
    {
        var unknownId = Guid.NewGuid();

        var response = await PutUserAsync(unknownId, new { countryIsoCode = "US", locale = "en-US" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await GetUserAsync(unknownId)).StatusCode);
        Assert.Empty(await GetAllUsersAsync());
    }

    [Theory]
    [Trait("TestCase", "TC-PUT-14")]
    [InlineData("")]
    [InlineData("{bad json")]
    public async Task UpdateUser_WithInvalidBody_ReturnsBadRequest(string body)
    {
        var created = await CreateUserAsync();

        var response = await PutRawJsonAsync(created.UserId, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    [Trait("TestCase", "TC-PUT-17")]
    public async Task UpdateUser_WithInvalidData_DoesNotChangeUser()
    {
        var created = await CreateUserAsync();
        Factory.FakeTimeProvider.Advance(TimeSpan.FromMinutes(1));

        var response = await PutUserAsync(created.UserId, new { countryIsoCode = "R1", locale = "en-US" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(created, await ReadUserAsync(await GetUserAsync(created.UserId)));
    }

    [Fact]
    [Trait("TestCase", "TC-PUT-18")]
    public async Task UpdateUser_WithServerFieldsInBody_IgnoresThem()
    {
        var created = await CreateUserAsync();
        Factory.FakeTimeProvider.Advance(TimeSpan.FromMinutes(1));
        var clientDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var response = await PutUserAsync(created.UserId, new
        {
            userId = Guid.NewGuid(),
            countryIsoCode = "US",
            locale = "en-US",
            dateCreated = clientDate,
            dateModified = clientDate
        });

        var updated = await ReadUserAsync(response);
        Assert.Equal(created.UserId, updated.UserId);
        Assert.Equal(created.DateCreated, updated.DateCreated);
        Assert.Equal(UtcNow, updated.DateModified);
    }
}
