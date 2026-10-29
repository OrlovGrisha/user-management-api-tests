using System.Net;
using UserManagement.Tests.Infrastructure;
using UserManagement.Tests.Models;
using Xunit.Abstractions;

namespace UserManagement.Tests.Tests.Integration;

public class PaginationTests : UsersApiTestBase
{
    private const int DefaultPageSize = 10;

    public PaginationTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    /// <summary>
    /// Creates users one minute apart and returns them in the order the API sorts them: newest first.
    /// </summary>
    private async Task<List<UserResponse>> CreateUsersNewestFirstAsync(int count)
    {
        var users = new List<UserResponse>();
        for (var i = 0; i < count; i++)
        {
            users.Add(await CreateUserAsync());
            Factory.FakeTimeProvider.Advance(TimeSpan.FromMinutes(1));
        }

        users.Reverse();
        return users;
    }

    private async Task<List<UserResponse>> GetPageAsync(string query)
    {
        var response = await GetUsersAsync(query);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return await ReadUsersAsync(response);
    }

    [Fact]
    [Trait("TestCase", "TC-PAGE-01")]
    public async Task GetUsers_WithoutPaginationParameters_ReturnsAllUsers()
    {
        var users = await CreateUsersNewestFirstAsync(DefaultPageSize + 2);

        var result = await GetPageAsync("");

        Assert.Equal(users.Count, result.Count);
    }

    [Theory]
    [Trait("TestCase", "TC-PAGE-02")]
    [Trait("TestCase", "TC-PAGE-03")]
    [InlineData(1, 3)]
    [InlineData(2, 3)]
    [InlineData(4, 3)]    // last, incomplete page
    [InlineData(2, 5)]
    public async Task GetUsers_WithPage_ReturnsExpectedSlice(int page, int pageSize)
    {
        var users = await CreateUsersNewestFirstAsync(10);
        var expected = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var result = await GetPageAsync($"?page={page}&pageSize={pageSize}");

        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("TestCase", "TC-PAGE-04")]
    public async Task GetUsers_AllPagesCombined_ContainAllUsersWithoutDuplicates()
    {
        var users = await CreateUsersNewestFirstAsync(10);

        var combined = new List<UserResponse>();
        for (var page = 1; page <= 4; page++)
            combined.AddRange(await GetPageAsync($"?page={page}&pageSize=3"));

        Assert.Equal(users, combined);
    }

    [Fact]
    [Trait("TestCase", "TC-PAGE-05")]
    public async Task GetUsers_UsersWithSameCreationDate_OrderIsStableAcrossRequests()
    {
        for (var i = 0; i < 6; i++)
            await CreateUserAsync();

        var firstRun = new List<UserResponse>();
        var secondRun = new List<UserResponse>();
        for (var page = 1; page <= 3; page++)
        {
            firstRun.AddRange(await GetPageAsync($"?page={page}&pageSize=2"));
            secondRun.AddRange(await GetPageAsync($"?page={page}&pageSize=2"));
        }

        Assert.Equal(firstRun, secondRun);
        Assert.Equal(6, firstRun.Select(u => u.UserId).Distinct().Count());
    }

    [Theory]
    [Trait("TestCase", "TC-PAGE-06")]
    [InlineData("?page=100&pageSize=3")]
    [InlineData("?page=1000000&pageSize=10")]    // largest allowed page, no int overflow
    public async Task GetUsers_PageOutOfRange_ReturnsEmptyArray(string query)
    {
        await CreateUsersNewestFirstAsync(5);

        var result = await GetPageAsync(query);

        Assert.Empty(result);
    }

    [Fact]
    [Trait("TestCase", "TC-PAGE-07")]
    public async Task GetUsers_PageSizeGreaterThanUserCount_ReturnsAllUsersOnFirstPage()
    {
        var users = await CreateUsersNewestFirstAsync(4);

        var result = await GetPageAsync($"?page=1&pageSize={DefaultPageSize}");

        Assert.Equal(users, result);
    }

    [Fact]
    [Trait("TestCase", "TC-PAGE-11")]
    public async Task GetUsers_OnlyPage_UsesDefaultPageSize()
    {
        var users = await CreateUsersNewestFirstAsync(DefaultPageSize + 2);

        var result = await GetPageAsync("?page=2");

        Assert.Equal(users.Skip(DefaultPageSize).ToList(), result);
    }

    [Fact]
    [Trait("TestCase", "TC-PAGE-11")]
    public async Task GetUsers_OnlyPageSize_ReturnsFirstPage()
    {
        var users = await CreateUsersNewestFirstAsync(5);

        var result = await GetPageAsync("?pageSize=3");

        Assert.Equal(users.Take(3).ToList(), result);
    }

    [Theory]
    [Trait("TestCase", "TC-PAGE-08")]
    [Trait("TestCase", "TC-PAGE-09")]
    [Trait("TestCase", "TC-PAGE-10")]
    [InlineData("?page=0&pageSize=3", "page")]
    [InlineData("?page=-1&pageSize=3", "page")]
    [InlineData("?page=1000001", "page")]
    [InlineData("?page=abc", "page")]
    [InlineData("?page=1&pageSize=0", "pageSize")]
    [InlineData("?page=1&pageSize=-1", "pageSize")]
    [InlineData("?page=1&pageSize=11", "pageSize")]
    [InlineData("?pageSize=abc", "pageSize")]
    public async Task GetUsers_InvalidPaginationParameters_ReturnsValidationError(string query, string field)
    {
        var response = await GetUsersAsync(query);

        await AssertValidationErrorAsync(response, field);
    }
}
