using System.Net;
using UserManagement.Tests.Infrastructure;
using Xunit.Abstractions;

namespace UserManagement.Tests.Tests.Integration;

public class CountryIsoCodeValidationTests : UsersApiTestBase
{
    private const string Field = "CountryIsoCode";
    private const string ValidLocale = "ru-RU";

    public CountryIsoCodeValidationTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Theory]
    [Trait("TestCase", "TC-CC-01")]
    [Trait("TestCase", "TC-CC-02")]
    [Trait("TestCase", "TC-CC-03")]
    [Trait("TestCase", "TC-CC-04")]
    [InlineData("RU", "RU")]
    [InlineData("us", "US")]
    [InlineData("De", "DE")]
    [InlineData("fR", "FR")]
    public async Task CreateUser_WithValidCountry_ReturnsCreatedWithUppercaseCountry(string countryIsoCode, string expected)
    {
        var response = await PostUserAsync(new { countryIsoCode, locale = ValidLocale });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(expected, (await ReadUserAsync(response)).CountryIsoCode);
    }

    [Theory]
    [InlineData("RU", "RU")]
    [InlineData("us", "US")]
    [InlineData("De", "DE")]
    [InlineData("fR", "FR")]
    public async Task UpdateUser_WithValidCountry_ReturnsOkWithUppercaseCountry(string countryIsoCode, string expected)
    {
        var user = await CreateUserAsync();

        var response = await PutUserAsync(user.UserId, new { countryIsoCode, locale = ValidLocale });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expected, (await ReadUserAsync(response)).CountryIsoCode);
    }

    [Theory]
    [Trait("TestCase", "TC-POST-19")]
    [InlineData("")]          // TC-CC-05 empty
    [InlineData("  ")]        // TC-CC-06 whitespace
    [InlineData("R")]         // TC-CC-07 too short
    [InlineData("RUS")]       // TC-CC-08 too long
    [InlineData("12")]        // TC-CC-09 digits
    [InlineData("R1")]        // TC-CC-10 letter and digit
    [InlineData("R$")]        // TC-CC-11 special characters
    [InlineData("!@")]        // TC-CC-11
    [InlineData("РУ")]        // TC-CC-12 Cyrillic
    [InlineData("ру")]        // TC-CC-12
    [InlineData("R U")]       // TC-CC-13 inner space
    [InlineData(" RU")]       // TC-CC-13 leading space
    [InlineData("RU ")]       // TC-CC-13 trailing space
    [InlineData("ÄÖ")]        // TC-CC-14 non-Latin letters
    [InlineData("RU\n")]      // TC-CC-15 trailing newline
    public async Task CreateUser_WithInvalidCountry_ReturnsValidationError(string countryIsoCode)
    {
        var response = await PostUserAsync(new { countryIsoCode, locale = ValidLocale });

        await AssertValidationErrorAsync(response, Field);
    }

    [Theory]
    [Trait("TestCase", "TC-PUT-15")]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("R")]
    [InlineData("RUS")]
    [InlineData("12")]
    [InlineData("R1")]
    [InlineData("R$")]
    [InlineData("!@")]
    [InlineData("РУ")]
    [InlineData("ру")]
    [InlineData("R U")]
    [InlineData(" RU")]
    [InlineData("RU ")]
    [InlineData("ÄÖ")]
    [InlineData("RU\n")]
    public async Task UpdateUser_WithInvalidCountry_ReturnsValidationError(string countryIsoCode)
    {
        var user = await CreateUserAsync();

        var response = await PutUserAsync(user.UserId, new { countryIsoCode, locale = ValidLocale });

        await AssertValidationErrorAsync(response, Field);
    }
}
