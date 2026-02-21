using Bogus;
using OrderManagement.Communication.Dtos.Customer;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Tests.Application.Builders;

public class CreateCustomerDtoBuilder
{
    private string _name;
    private string _email;
    private string _phone;
    private string _address;

    private readonly Faker _faker;

    public CreateCustomerDtoBuilder()
    {
        _faker = new Faker("pt_BR");
        _name = _faker.Person.FullName;
        _email = _faker.Internet.Email().ToLower();
        _phone = "1199999999999";
        _address = _faker.Address.FullAddress();
    }

    public CreateCustomerDtoBuilder WithInvalidEmail()
    {
        _email = "invalid-email";
        return this;
    }
    public CreateCustomerDto Build()
    { 
        return new CreateCustomerDto
        {
            Name = _name,
            Email = _email,
            Phone = _phone,
            Address = _address
        };
    }

}
