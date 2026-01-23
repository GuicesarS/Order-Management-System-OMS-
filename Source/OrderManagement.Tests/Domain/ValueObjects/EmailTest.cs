using FluentAssertions;
using OrderManagement.Domain.Exception;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Tests.Domain.ValueObjects;

public class EmailTest
{

    [Fact]
    public void Create_ShouldCreateEmail_WhenEmailIsValid()
    {
        var value = "teste@gmail.com";

        var email = Email.Create(value);

        email.Should().NotBeNull();
        email.Value.Should().Be(value);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenEmailIsNull()
    {
        Action act = () => Email.Create(null!);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("Email is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("invalid-email")]
    public void Create_ShouldThrowException_WhenEmailFormatIsInvalid(string value)
    {
        Action act = () => Email.Create(value);

        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage("*Email*");
    }

}
