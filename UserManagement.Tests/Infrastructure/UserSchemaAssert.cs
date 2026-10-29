using System.Text.Json;

namespace UserManagement.Tests.Infrastructure;

public static class UserSchemaAssert
{
    private static readonly string[] ExpectedProperties =
        ["countryIsoCode", "dateCreated", "dateModified", "locale", "userId"];

    public static void IsValidUser(JsonElement user)
    {
        Assert.Equal(JsonValueKind.Object, user.ValueKind);

        var actualProperties = user.EnumerateObject().Select(p => p.Name).Order();
        Assert.Equal(ExpectedProperties, actualProperties);

        var userId = user.GetProperty("userId");
        Assert.Equal(JsonValueKind.String, userId.ValueKind);
        Assert.True(Guid.TryParse(userId.GetString(), out _), "userId is not a valid GUID.");

        Assert.Equal(JsonValueKind.String, user.GetProperty("countryIsoCode").ValueKind);
        Assert.Equal(JsonValueKind.String, user.GetProperty("locale").ValueKind);

        var dateCreated = user.GetProperty("dateCreated");
        Assert.True(dateCreated.TryGetDateTime(out _), "dateCreated is not a valid date.");

        var dateModified = user.GetProperty("dateModified");
        Assert.True(
            dateModified.ValueKind == JsonValueKind.Null || dateModified.TryGetDateTime(out _),
            "dateModified must be null or a valid date.");
    }
}
