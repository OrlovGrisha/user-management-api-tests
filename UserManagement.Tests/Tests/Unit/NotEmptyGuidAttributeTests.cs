using UserManagement.API.Validation;

namespace UserManagement.Tests.Tests.Unit;

public class NotEmptyGuidAttributeTests
{
    private readonly NotEmptyGuidAttribute _attribute = new();

    [Fact]
    public void IsValid_WithNonEmptyGuid_ReturnsTrue()
    {
        Assert.True(_attribute.IsValid(Guid.NewGuid()));
    }

    [Fact]
    public void IsValid_WithEmptyGuid_ReturnsFalse()
    {
        Assert.False(_attribute.IsValid(Guid.Empty));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6")]
    [InlineData(42)]
    public void IsValid_WithNonGuidValue_ReturnsFalse(object? value)
    {
        Assert.False(_attribute.IsValid(value));
    }

    [Fact]
    public void FormatErrorMessage_ContainsFieldName()
    {
        var message = _attribute.FormatErrorMessage("userId");

        Assert.Equal("userId must not be an empty GUID.", message);
    }
}
