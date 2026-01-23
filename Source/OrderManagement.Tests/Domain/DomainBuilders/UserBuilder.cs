using Bogus;
using OrderManagement.Domain.Entities.User;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Tests.Domain.DomainBuilders;

public class UserBuilder
{
    private readonly Faker _faker;

    private string _validName;
    private Email _validEmail;
    private UserRole _userRole;
    public UserBuilder()
    {
        _faker = new Faker("pt_BR");

        _validName = _faker.Person.FullName;
        _validEmail = Email.Create(_faker.Internet.Email());
        _userRole = _faker.PickRandom<UserRole>();
    }

    public User Build() => 
        new User(_validName, _validEmail, _userRole);
    public User BuildInvalidUserWithEmptyName() => 
        new User("", _validEmail, _userRole);
    public User BuildInvalidUserWithNullEmail()
        => new User(_validName, null!, _userRole);
    public User BuildWithInvalidRole()
        => new User(_validName, _validEmail, (UserRole)999);



}
