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

    [Fact]
    public void Create_ShouldThrowException_WhenEmailFormatIsInvalid()
    {
        
        Action act = () => Email.Create("xxxx");

        act.Should()
           .Throw<DomainValidationException>()
           .WithMessage("*invalid*");
    }
}
