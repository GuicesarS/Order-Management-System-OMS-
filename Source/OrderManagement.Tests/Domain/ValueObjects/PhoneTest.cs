using FluentAssertions;
using OrderManagement.Domain.Exception;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Tests.Domain.ValueObjects;

public class PhoneTest
{

    [Fact]
    public void Create_ShouldBeSuccessful_WhenPhoneIsValid()
    {
        var phone = Phone.Create("5531998888888");

        phone.Should().NotBeNull();
        
    }

    [Fact]
    public void Create_ShouldThrowException_WhenPhoneIsNull()
    {
        Action act = () => Phone.Create(null!);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("Phone is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("999")]
    public void Create_ShouldThrowException_WhenPhoneIsInvalid(string value)
    {
        Action act = () => Phone.Create(value);

        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage("*Phone*");
    }
}
