namespace UserManagement.API.Models;

public class User
{
    public Guid UserId { get; }
    public string CountryIsoCode { get; }
    public DateTime DateCreated { get; }
    public DateTime? DateModified { get; }
    public string Locale { get; }

    private User(Guid userId, string countryIsoCode, DateTime dateCreated, DateTime? dateModified, string locale)
    {
        UserId = userId;
        CountryIsoCode = countryIsoCode.ToUpperInvariant();
        DateCreated = dateCreated;
        DateModified = dateModified;
        Locale = locale;
    }

    public static User Create(string countryIsoCode, string locale, DateTime dateCreated)
    {
        return new User(Guid.NewGuid(), countryIsoCode, dateCreated, null, locale);
    }

    public User Update(string countryIsoCode, string locale, DateTime dateModified)
    {
        return new User(UserId, countryIsoCode, DateCreated, dateModified, locale);
    }
}
