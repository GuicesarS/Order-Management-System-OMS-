using Bogus;
using OrderManagement.Domain.Entities.Customer;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Tests.Domain.DomainBuilders;

public class CustomerBuilder
{
    private readonly Faker _faker;

    private string _validName;
    private Email _validEmail;
    private Phone _validPhone;
    private string _validAddress;

    public CustomerBuilder()
    {

        _faker = new Faker("pt_BR");

        _validName = _faker.Person.FullName;
        _validEmail = Email.Create(_faker.Internet.Email());
        _validPhone = Phone.Create(GenerateValidPhone(_faker));
        _validAddress = _faker.Address.StreetAddress();
    }

    public Customer Build()
        => new Customer(_validName, _validEmail, _validPhone, _validAddress);

    public Customer BuildWithEmptyName()
        => new Customer("", _validEmail, _validPhone, _validAddress);

    public Customer BuildWithNullEmail()
        => new Customer(_validName, null!, _validPhone, _validAddress);

    public Customer BuildWithNullPhone()
        => new Customer(_validName, _validEmail, null!, _validAddress);

    public Customer BuildWithInvalidPhone()
        => new Customer(_validName, _validEmail, Phone.Create("123"), _validAddress);

    public Customer BuildWithEmptyAdress()
        => new Customer(_validName, _validEmail, _validPhone, "");

    private static string GenerateValidPhone(Faker f)
    {
        var ddi = "55";
        var ddd = f.Random.Number(11, 99).ToString();
        var number = f.Random.Number(900000000, 999999999).ToString();

        return $"{ddi}{ddd}{number}";
    }

}
