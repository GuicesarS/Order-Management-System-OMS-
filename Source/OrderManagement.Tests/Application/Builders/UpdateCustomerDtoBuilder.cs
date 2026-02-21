using Bogus;
using OrderManagement.Communication.Dtos.Customer;

namespace OrderManagement.Tests.Application.Builders;

public class UpdateCustomerDtoBuilder
{
    private string? _name;
    private string? _email;
    private string? _phone;
    private string? _address;

    private readonly Faker _faker;

    public UpdateCustomerDtoBuilder()
    {
        _faker = new Faker("pt_BR");

        _name = _faker.Person.FullName;
        _email = _faker.Internet.Email();
        _phone = "5511999999999";
        _address = _faker.Address.FullAddress();
    }

    public UpdateCustomerDto Build()
    {
        return new UpdateCustomerDto
        {
            Name = _name,
            Email = _email,
            Phone = _phone,
            Address = _address
        };
    }
}
