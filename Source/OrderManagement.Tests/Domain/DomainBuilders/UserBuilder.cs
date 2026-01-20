using Bogus;
using OrderManagement.Domain.Entities.User;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.ValueObjects;
using System.Security.Cryptography.X509Certificates;

namespace OrderManagement.Tests.Domain.DomainBuilders;

public class UserBuilder
{
    private readonly Faker<User> _faker;
    public UserBuilder()
    {
        _faker = new Faker<User>()
             .CustomInstantiator(f => 
             new User(f.Person.FirstName,Email.Create(f.Internet.Email()), f.PickRandom<UserRole>()
             ));

    }

    public User Build() => _faker.Generate();
    public User BuildInvalidUserWithEmptyName() => 
        new User("", Email.Create("teste@gmail.com"), UserRole.Admin);
    public User BuildInvalidUserWithNullEmail()
        => new User("Guilherme", null!, UserRole.Admin);
    public User BuildInvalidUserWithInvalidRole()
        => new User("Guilherme", Email.Create("teste@email.com"), (UserRole)999);



}
