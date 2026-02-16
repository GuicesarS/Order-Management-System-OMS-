using Bogus;
using OrderManagement.Domain.Entities.User;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Tests.Domain.Builders;

public class UserBuilder
{
    private readonly Faker _faker;

    private string _validName;
    private Email _validEmail;
    private string _password;
    private UserRole _userRole;
    public UserBuilder()
    {
        _faker = new Faker("pt_BR");

        _validName = _faker.Person.FullName;
        _validEmail = Email.Create(_faker.Internet.Email());
        _password = _faker.Internet.Password();
        _userRole = _faker.PickRandom<UserRole>();
    }

    public User Build() => 
        new User(_validName, _validEmail, _password, _userRole);
    public User BuildInvalidUserWithEmptyName() => 
        new User("", _validEmail, _password, _userRole);
    public User BuildInvalidUserWithNullEmail()
        => new User(_validName, null!, _password, _userRole);
    public User BuildWithInvalidRole()
        => new User(_validName, _validEmail, _password, (UserRole)999);
    public User BuildWithInvalidPassword()
        => new User(_validName, _validEmail, string.Empty, _userRole); // Verificar se está correto


}
