namespace UserManagement.Tests.Models;

public record UserResponse(Guid UserId, string CountryIsoCode, DateTime DateCreated, DateTime? DateModified, string Locale);