using System.Net;
using UserManagement.Tests.Infrastructure;
using Xunit.Abstractions;

namespace UserManagement.Tests.Tests.Integration;

public class LocaleValidationTests : UsersApiTestBase
{
    private const string Field = "Locale";
    private const string ValidCountry = "RU";

    public LocaleValidationTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Theory]
    [Trait("TestCase", "TC-LOC-01")]
    [Trait("TestCase", "TC-LOC-02")]
    [Trait("TestCase", "TC-LOC-03")]
    [InlineData("ru-RU")]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    public async Task CreateUser_WithValidLocale_ReturnsCreatedWithSameLocale(string locale)
    {
        var response = await PostUserAsync(new { countryIsoCode = ValidCountry, locale });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(locale, (await ReadUserAsync(response)).Locale);
    }

    [Theory]
    [InlineData("ru-RU")]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    public async Task UpdateUser_WithValidLocale_ReturnsOkWithSameLocale(string locale)
    {
        var user = await CreateUserAsync();

        var response = await PutUserAsync(user.UserId, new { countryIsoCode = ValidCountry, locale });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(locale, (await ReadUserAsync(response)).Locale);
    }

    [Theory]
    [Trait("TestCase", "TC-POST-20")]
    [InlineData("")]          // TC-LOC-04 empty
    [InlineData("     ")]     // TC-LOC-05 whitespace
    [InlineData("RU-RU")]     // TC-LOC-06 uppercase language
    [InlineData("ru-ru")]     // TC-LOC-07 lowercase region
    [InlineData("RU-ru")]     // TC-LOC-08 swapped case
    [InlineData("ru_RU")]     // TC-LOC-09 underscore instead of hyphen
    [InlineData("ruRU")]      // TC-LOC-10 no separator
    [InlineData("ru")]        // TC-LOC-11 language only
    [InlineData("rus-RU")]    // TC-LOC-12 wrong length
    [InlineData("ru-RUS")]    // TC-LOC-12
    [InlineData("r-RU")]      // TC-LOC-12
    [InlineData("12-34")]     // TC-LOC-13 digits
    [InlineData("ru-12")]     // TC-LOC-13
    [InlineData("r$-RU")]     // TC-LOC-14 special characters
    [InlineData("ru-R!")]     // TC-LOC-14
    [InlineData("ру-РУ")]     // TC-LOC-15 Cyrillic
    [InlineData(" ru-RU")]    // TC-LOC-16 leading space
    [InlineData("ru-RU ")]    // TC-LOC-16 trailing space
    public async Task CreateUser_WithInvalidLocale_ReturnsValidationError(string locale)
    {
        var response = await PostUserAsync(new { countryIsoCode = ValidCountry, locale });

        await AssertValidationErrorAsync(response, Field);
    }

    [Theory]
    [Trait("TestCase", "TC-PUT-16")]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData("RU-RU")]
    [InlineData("ru-ru")]
    [InlineData("RU-ru")]
    [InlineData("ru_RU")]
    [InlineData("ruRU")]
    [InlineData("ru")]
    [InlineData("rus-RU")]
    [InlineData("ru-RUS")]
    [InlineData("r-RU")]
    [InlineData("12-34")]
    [InlineData("ru-12")]
    [InlineData("r$-RU")]
    [InlineData("ru-R!")]
    [InlineData("ру-РУ")]
    [InlineData(" ru-RU")]
    [InlineData("ru-RU ")]
    public async Task UpdateUser_WithInvalidLocale_ReturnsValidationError(string locale)
    {
        var user = await CreateUserAsync();

        var response = await PutUserAsync(user.UserId, new { countryIsoCode = ValidCountry, locale });

        await AssertValidationErrorAsync(response, Field);
    }
}
