using Bogus;
using OrderManagement.Communication.Dtos.User;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Tests.Application.Builders;

public class CreateUserDtoBuilder
{
    private string _name;
    private string _email;
    private string _password;
    private string _role;
    private readonly Faker _faker;

    public CreateUserDtoBuilder()
    {
        _faker = new Faker("pt_BR");
        _name = _faker.Person.FullName;
        _email = _faker.Internet.Email().ToLower();
        _password = _faker.Internet.Password();
        _role = _faker.PickRandom<UserRole>().ToString();
    }

    public CreateUserDtoBuilder WithRole(UserRole role)
    {
        _role = role.ToString();
        return this;
    }

    public CreateUserDtoBuilder WithInvalidRole()
    {
        _role = "RoleInexistente";
        return this;
    }

    public CreateUserDtoBuilder WithInvalidEmail()
    {
        _email = "invalid-email";
        return this;
    }

    public CreateUserDto Build()
    {
        return new CreateUserDto
        {
            Name = _name,
            Email = _email,
            Password = _password,
            Role = _role
        };
    }
}

