using System.ComponentModel.DataAnnotations;

namespace UserManagement.API.DTOs;

public record UserDto(
    [Required(ErrorMessage = "CountryIsoCode is required.")]
    [RegularExpression("^[a-zA-Z]{2}$", ErrorMessage = "CountryIsoCode must consist of exactly 2 Latin letters, e.g. RU.")]
    string CountryIsoCode,

    [Required(ErrorMessage = "Locale is required.")]
    [RegularExpression("^[a-z]{2}-[A-Z]{2}$", ErrorMessage = "Locale must be in format xx-XX, e.g. ru-RU.")]
    string Locale);
