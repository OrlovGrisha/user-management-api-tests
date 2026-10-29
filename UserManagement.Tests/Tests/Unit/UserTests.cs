using UserManagement.API.Models;

namespace UserManagement.Tests.Tests.Unit;

public class UserTests
{
    private static readonly DateTime CreatedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ModifiedAt = CreatedAt.AddMinutes(5);

    [Fact]
    public void Create_WithValidData_SetsAllProperties()
    {
        var user = User.Create("RU", "ru-RU", CreatedAt);

        Assert.NotEqual(Guid.Empty, user.UserId);
        Assert.Equal("RU", user.CountryIsoCode);
        Assert.Equal("ru-RU", user.Locale);
        Assert.Equal(CreatedAt, user.DateCreated);
        Assert.Null(user.DateModified);
    }

    [Theory]
    [InlineData("ru", "RU")]
    [InlineData("Us", "US")]
    [InlineData("dE", "DE")]
    public void Create_WithAnyCaseCountry_StoresUppercase(string countryIsoCode, string expected)
    {
        var user = User.Create(countryIsoCode, "ru-RU", CreatedAt);

        Assert.Equal(expected, user.CountryIsoCode);
    }

    [Fact]
    public void Create_CalledTwice_GeneratesDifferentIds()
    {
        var first = User.Create("RU", "ru-RU", CreatedAt);
        var second = User.Create("RU", "ru-RU", CreatedAt);

        Assert.NotEqual(first.UserId, second.UserId);
    }

    [Fact]
    public void Update_WithNewData_ReturnsUserWithUpdatedFieldsAndSameIdentity()
    {
        var user = User.Create("RU", "ru-RU", CreatedAt);

        var updated = user.Update("us", "en-US", ModifiedAt);

        Assert.Equal(user.UserId, updated.UserId);
        Assert.Equal(CreatedAt, updated.DateCreated);
        Assert.Equal("US", updated.CountryIsoCode);
        Assert.Equal("en-US", updated.Locale);
        Assert.Equal(ModifiedAt, updated.DateModified);
    }

    [Fact]
    public void Update_DoesNotMutateOriginalUser()
    {
        var user = User.Create("RU", "ru-RU", CreatedAt);

        var updated = user.Update("US", "en-US", ModifiedAt);

        Assert.NotSame(user, updated);
        Assert.Equal("RU", user.CountryIsoCode);
        Assert.Equal("ru-RU", user.Locale);
        Assert.Null(user.DateModified);
    }
}
