using UserManagement.Tests.Infrastructure;
using Xunit.Abstractions;

namespace UserManagement.Tests.Tests.Integration;

/// <summary>
/// Each run starts from an empty repository and creates its own number of users.
/// If state leaked between tests, the counts would not match.
/// </summary>
public class IsolationTests : UsersApiTestBase
{
    public IsolationTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Theory]
    [Trait("TestCase", "TC-ISO-01")]
    [Trait("TestCase", "TC-ISO-02")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task EachTest_StartsWithEmptyRepository(int usersToCreate)
    {
        Assert.Empty(await GetAllUsersAsync());

        for (var i = 0; i < usersToCreate; i++)
            await CreateUserAsync();

        Assert.Equal(usersToCreate, (await GetAllUsersAsync()).Count);
    }
}
