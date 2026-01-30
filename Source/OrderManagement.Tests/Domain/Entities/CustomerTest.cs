using FluentAssertions;
using OrderManagement.Domain.Exception;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Tests.Domain.Builders;

namespace OrderManagement.Tests.Domain.Entities;

public class CustomerTest
{
    // Constructor Tests

    [Fact]
    public void Create_ShouldBeSuccessful_ForValidValues()
    {
        var customer = new CustomerBuilder().Build();

        customer.Should().NotBeNull();
        
    }

    [Fact]
    public void Create_SouldThowException_EmptyName()
    {
        var customer = new CustomerBuilder();

        Action act = () => customer.BuildWithEmptyName();

        act.Should().Throw<DomainValidationException>()
            .WithMessage("Name cannot be null or empty.");

    }

    [Fact]
    public void Create_SouldThowException_NullEmail()
    {
        var customer = new CustomerBuilder();

        Action act = () => customer.BuildWithNullEmail();

        act.Should().Throw<DomainValidationException>()
            .WithMessage("Email is required.");

    }

    [Fact]
    public void Create_ShouldThowException_WhenPhoneIsNull()
    {
        
        Action act = () => new CustomerBuilder().BuildWithNullPhone();

        act.Should().Throw<DomainValidationException>()
            .WithMessage("Phone is required.");

    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("abc")]
    public void Phone_ShouldThrowException_WhenInvalid(string value)
    {
        Action act = () => Phone.Create(value);

        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage("*Phone*");
    }

    [Fact]
    public void Create_ShouldThowException_EmptyAdress()
    {
        var customer = new CustomerBuilder();

        Action act = () => customer.BuildWithEmptyAdress();
    
        act.Should().Throw<DomainValidationException>()
            .WithMessage("Address cannot be null or empty.");

    }

    // Update Tests

    [Fact]
    public void Update_ShouldBeSuccessfull_ForValidValues()
    {
        var customer = new CustomerBuilder().Build();

        customer.UpdateCustomerProfile(
            "Guilherme", 
            Email.Create("guilherme@gmail.com"), 
            Phone.Create("5531999999999"), 
            "Rua da Angola, 774");

        customer.Should().NotBeNull();
        customer.Name.Should().Be("Guilherme");
        customer.Email.Value.Should().Be("guilherme@gmail.com"); 
        customer.Phone.Value.Should().Be("5531999999999");
        customer.Address.Should().Be("Rua da Angola, 774");

    }

    [Theory]
    [InlineData("", "teste@gmail.com", "5531999999999", "Rua da Angola, 774", "Name cannot be null or empty.")]
    [InlineData("Guilherme", "teste@gmail.com", "5531999999999", "", "Address cannot be null or empty.")]
    public void Update_ShouldThrowException_WhenValuesAreInvalids(
    string? name,
    string email,
    string phone,
    string address,
    string errorMessage)
    {
        var customer = new CustomerBuilder().Build();

        Action act = () => customer.UpdateCustomerProfile(
            name,
            Email.Create(email),           
            Phone.Create(phone),           
            address
        );

        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage($"*{errorMessage}*");
    }

}
