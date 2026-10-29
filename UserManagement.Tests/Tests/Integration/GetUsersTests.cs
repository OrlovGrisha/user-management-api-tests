using System.Net;
using UserManagement.Tests.Infrastructure;
using Xunit.Abstractions;

namespace UserManagement.Tests.Tests.Integration;

public class GetUsersTests : UsersApiTestBase
{
    public GetUsersTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    [Trait("TestCase", "TC-LIST-01")]
    [Trait("TestCase", "TC-ISO-01")]
    public async Task GetUsers_NoUsers_ReturnsEmptyArray()
    {
        var response = await GetUsersAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("[]", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    [Trait("TestCase", "TC-LIST-02")]
    [Trait("TestCase", "TC-LIST-03")]
    [Trait("TestCase", "TC-LIST-04")]
    public async Task GetUsers_SeveralUsers_ReturnsAllOfThem()
    {
        var created = new[]
        {
            await CreateUserAsync("RU", "ru-RU"),
            await CreateUserAsync("DE", "de-DE"),
            await CreateUserAsync("US", "en-US")
        };

        var users = await GetAllUsersAsync();

        Assert.Equal(created.Length, users.Count);
        Assert.All(created, user => Assert.Contains(user, users));
    }

    [Fact]
    [Trait("TestCase", "TC-LIST-05")]
    public async Task GetUsers_AfterUpdate_ReturnsUpdatedData()
    {
        var created = await CreateUserAsync();
        Factory.FakeTimeProvider.Advance(TimeSpan.FromMinutes(1));
        var updated = await ReadUserAsync(await PutUserAsync(created.UserId, new { countryIsoCode = "US", locale = "en-US" }));

        var users = await GetAllUsersAsync();

        Assert.Equal(updated, Assert.Single(users));
    }
}
