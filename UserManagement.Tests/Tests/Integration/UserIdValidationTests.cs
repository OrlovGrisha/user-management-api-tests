using UserManagement.Tests.Infrastructure;
using Xunit.Abstractions;

namespace UserManagement.Tests.Tests.Integration;

public class UserIdValidationTests : UsersApiTestBase
{
    private const string Field = "userId";
    private static readonly object ValidBody = new { countryIsoCode = "RU", locale = "ru-RU" };

    public UserIdValidationTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    [Trait("TestCase", "TC-GET-05")]
    public async Task GetUser_EmptyGuid_ReturnsValidationError()
    {
        var response = await GetUserAsync(Guid.Empty);

        await AssertValidationErrorAsync(response, Field);
    }

    [Fact]
    [Trait("TestCase", "TC-PUT-12")]
    public async Task UpdateUser_EmptyGuid_ReturnsValidationError()
    {
        var response = await PutUserAsync(Guid.Empty, ValidBody);

        await AssertValidationErrorAsync(response, Field);
    }

    [Theory]
    [Trait("TestCase", "TC-GET-06")]
    [Trait("TestCase", "TC-GET-07")]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("not-a-guid")]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa")]      // one character short
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6a")]    // one character too long
    public async Task GetUser_MalformedId_ReturnsValidationError(string userId)
    {
        var response = await GetUserAsync(userId);

        await AssertValidationErrorAsync(response, Field);
    }

    [Theory]
    [Trait("TestCase", "TC-PUT-13")]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("not-a-guid")]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa")]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6a")]
    public async Task UpdateUser_MalformedId_ReturnsValidationError(string userId)
    {
        var response = await PutUserAsync(userId, ValidBody);

        await AssertValidationErrorAsync(response, Field);
    }
}
