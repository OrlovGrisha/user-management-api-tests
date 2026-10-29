using System.Net;
using UserManagement.Tests.Infrastructure;
using Xunit.Abstractions;

namespace UserManagement.Tests.Tests.Integration;

public class GetUserTests : UsersApiTestBase
{
    public GetUserTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    [Trait("TestCase", "TC-GET-01")]
    [Trait("TestCase", "TC-GET-02")]
    public async Task GetUser_ExistingId_ReturnsUser()
    {
        var created = await CreateUserAsync("DE", "de-DE");

        var response = await GetUserAsync(created.UserId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created, await ReadUserAsync(response));
    }

    [Fact]
    [Trait("TestCase", "TC-GET-03")]
    public async Task GetUser_SeveralUsersExist_ReturnsRequestedOne()
    {
        await CreateUserAsync("RU", "ru-RU");
        var requested = await CreateUserAsync("DE", "de-DE");
        await CreateUserAsync("FR", "fr-FR");

        var response = await GetUserAsync(requested.UserId);

        Assert.Equal(requested, await ReadUserAsync(response));
    }

    [Fact]
    [Trait("TestCase", "TC-GET-04")]
    public async Task GetUser_NonExistentId_ReturnsNotFound()
    {
        await CreateUserAsync();

        var response = await GetUserAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
