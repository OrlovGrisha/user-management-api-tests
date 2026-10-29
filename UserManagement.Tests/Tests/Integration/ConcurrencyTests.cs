using System.Net;
using UserManagement.Tests.Infrastructure;
using Xunit.Abstractions;

namespace UserManagement.Tests.Tests.Integration;

public class ConcurrencyTests : UsersApiTestBase
{
    private const int RequestCount = 50;

    public ConcurrencyTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {

    }

    [Fact]
    [Trait("TestCase", "TC-CONC-01")]
    public async Task CreateUser_ParallelRequests_AllUsersCreatedWithUniqueIds()
    {
        var responses = await Task.WhenAll(Enumerable.Range(0, RequestCount)
            .Select(_ => PostUserAsync(new { countryIsoCode = "RU", locale = "ru-RU" })));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.Created, r.StatusCode));
        var created = await Task.WhenAll(responses.Select(ReadUserAsync));
        Assert.Equal(RequestCount, created.Select(u => u.UserId).Distinct().Count());
        Assert.Equal(RequestCount, (await GetAllUsersAsync()).Count);
    }

    [Fact]
    [Trait("TestCase", "TC-CONC-02")]
    public async Task UpdateUser_ParallelRequests_FinalStateMatchesOneOfThem()
    {
        var user = await CreateUserAsync();
        var bodies = new[]
        {
            new { countryIsoCode = "US", locale = "en-US" },
            new { countryIsoCode = "DE", locale = "de-DE" },
            new { countryIsoCode = "FR", locale = "fr-FR" },
            new { countryIsoCode = "GB", locale = "en-GB" }
        };

        var responses = await Task.WhenAll(Enumerable.Range(0, RequestCount)
            .Select(i => PutUserAsync(user.UserId, bodies[i % bodies.Length])));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        var final = await ReadUserAsync(await GetUserAsync(user.UserId));
        Assert.Contains(bodies, b => b.countryIsoCode == final.CountryIsoCode && b.locale == final.Locale);
        Assert.Single(await GetAllUsersAsync());
    }

    [Fact]
    [Trait("TestCase", "TC-CONC-03")]
    public async Task CreateAndList_ParallelRequests_NoServerErrors()
    {
        var requests = Enumerable.Range(0, RequestCount)
            .Select(i => i % 2 == 0
                ? PostUserAsync(new { countryIsoCode = "RU", locale = "ru-RU" })
                : GetUsersAsync());

        var responses = await Task.WhenAll(requests);

        Assert.All(responses, r => Assert.True(r.IsSuccessStatusCode, $"Unexpected status {r.StatusCode}"));
        Assert.Equal(RequestCount / 2, (await GetAllUsersAsync()).Count);
    }
}
