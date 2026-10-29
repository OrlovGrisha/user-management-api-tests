using System.ComponentModel.DataAnnotations;

namespace UserManagement.API.Validation;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public sealed class NotEmptyGuidAttribute : ValidationAttribute
{
    public NotEmptyGuidAttribute()
        : base("{0} must not be an empty GUID.")
    {
    }

    public override bool IsValid(object? value)
    {
        return value is Guid guid && guid != Guid.Empty;
    }
}
