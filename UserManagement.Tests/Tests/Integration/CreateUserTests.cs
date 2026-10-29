using System.Net;
using UserManagement.Tests.Infrastructure;
using Xunit.Abstractions;

namespace UserManagement.Tests.Tests.Integration;

public class CreateUserTests : UsersApiTestBase
{
    public CreateUserTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    [Trait("TestCase", "TC-POST-01")]
    [Trait("TestCase", "TC-POST-02")]
    [Trait("TestCase", "TC-POST-03")]
    [Trait("TestCase", "TC-POST-04")]
    [Trait("TestCase", "TC-POST-05")]
    [Trait("TestCase", "TC-POST-09")]
    public async Task CreateUser_WithValidData_ReturnsCreatedUser()
    {
        var response = await PostUserAsync(new { countryIsoCode = "RU", locale = "ru-RU" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await ReadUserAsync(response);
        Assert.NotEqual(Guid.Empty, user.UserId);
        Assert.Equal("RU", user.CountryIsoCode);
        Assert.Equal("ru-RU", user.Locale);
        Assert.Equal(UtcNow, user.DateCreated);
        Assert.Null(user.DateModified);
    }

    [Fact]
    [Trait("TestCase", "TC-POST-06")]
    public async Task CreateUser_WithValidData_ReturnsLocationOfCreatedUser()
    {
        var response = await PostUserAsync(new { countryIsoCode = "RU", locale = "ru-RU" });

        var user = await ReadUserAsync(response);
        Assert.NotNull(response.Headers.Location);
        Assert.EndsWith($"{UsersUrl}/{user.UserId}", response.Headers.Location.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [Trait("TestCase", "TC-POST-07")]
    [Trait("TestCase", "TC-POST-08")]
    [InlineData("ru", "RU")]
    [InlineData("rU", "RU")]
    [InlineData("Ru", "RU")]
    [InlineData("RU", "RU")]
    public async Task CreateUser_WithAnyCaseCountry_StoresUppercase(string countryIsoCode, string expected)
    {
        var created = await CreateUserAsync(countryIsoCode);

        Assert.Equal(expected, created.CountryIsoCode);
        var stored = await ReadUserAsync(await GetUserAsync(created.UserId));
        Assert.Equal(expected, stored.CountryIsoCode);
    }

    [Fact]
    [Trait("TestCase", "TC-POST-10")]
    public async Task CreateUser_SameDataTwice_CreatesTwoUsersWithDifferentIds()
    {
        var first = await CreateUserAsync();
        var second = await CreateUserAsync();

        Assert.NotEqual(first.UserId, second.UserId);
        Assert.Equal(2, (await GetAllUsersAsync()).Count);
    }

    [Fact]
    [Trait("TestCase", "TC-POST-11")]
    public async Task CreateUser_WithServerFieldsInBody_IgnoresThem()
    {
        var clientId = Guid.NewGuid();
        var clientDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var response = await PostUserAsync(new
        {
            userId = clientId,
            countryIsoCode = "RU",
            locale = "ru-RU",
            dateCreated = clientDate,
            dateModified = clientDate
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await ReadUserAsync(response);
        Assert.NotEqual(clientId, user.UserId);
        Assert.Equal(UtcNow, user.DateCreated);
        Assert.Null(user.DateModified);
    }

    [Fact]
    [Trait("TestCase", "TC-POST-12")]
    public async Task CreateUser_ThenGetById_ReturnsSameUser()
    {
        var created = await CreateUserAsync();

        var response = await GetUserAsync(created.UserId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created, await ReadUserAsync(response));
    }

    [Theory]
    [Trait("TestCase", "TC-POST-13")]
    [Trait("TestCase", "TC-POST-14")]
    [InlineData("")]
    [InlineData("{bad json")]
    [InlineData("[]")]
    [InlineData("\"RU\"")]
    public async Task CreateUser_WithInvalidBody_ReturnsBadRequest(string body)
    {
        var response = await PostRawJsonAsync(body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    [Trait("TestCase", "TC-POST-15")]
    public async Task CreateUser_WithoutCountry_ReturnsValidationError()
    {
        var response = await PostUserAsync(new { locale = "ru-RU" });

        await AssertValidationErrorAsync(response, "CountryIsoCode");
    }

    [Fact]
    [Trait("TestCase", "TC-POST-16")]
    public async Task CreateUser_WithoutLocale_ReturnsValidationError()
    {
        var response = await PostUserAsync(new { countryIsoCode = "RU" });

        await AssertValidationErrorAsync(response, "Locale");
    }

    [Fact]
    [Trait("TestCase", "TC-POST-17")]
    public async Task CreateUser_WithNullCountry_ReturnsValidationError()
    {
        var response = await PostUserAsync(new { countryIsoCode = (string?)null, locale = "ru-RU" });

        await AssertValidationErrorAsync(response, "CountryIsoCode");
    }

    [Fact]
    [Trait("TestCase", "TC-POST-18")]
    public async Task CreateUser_WithNullLocale_ReturnsValidationError()
    {
        var response = await PostUserAsync(new { countryIsoCode = "RU", locale = (string?)null });

        await AssertValidationErrorAsync(response, "Locale");
    }

    [Fact]
    [Trait("TestCase", "TC-POST-21")]
    public async Task CreateUser_WithBothFieldsInvalid_ReturnsErrorsForBothFields()
    {
        var response = await PostUserAsync(new { countryIsoCode = "R1", locale = "RU-ru" });

        await AssertValidationErrorAsync(response, "CountryIsoCode");
        await AssertValidationErrorAsync(response, "Locale");
    }

    [Fact]
    [Trait("TestCase", "TC-POST-22")]
    public async Task CreateUser_WithInvalidData_DoesNotCreateUser()
    {
        await PostUserAsync(new { countryIsoCode = "R1", locale = "ru-RU" });

        Assert.Empty(await GetAllUsersAsync());
    }
}
