using Bogus;
using OrderManagement.Communication.Dtos.User;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Tests.Application.Builders;

public class UpdateUserDtoBuilder
{
    private string? _name;
    private string? _email;
    private string? _password;
    private string? _role;
    private readonly Faker _faker;

    public UpdateUserDtoBuilder()
    {
        _faker = new Faker("pt_BR");
        _name = _faker.Person.FullName;
        _email = _faker.Internet.Email().ToLower();
        _password = _faker.Internet.Password();
        _role = _faker.PickRandom<UserRole>().ToString();
    }

    public UpdateUserDtoBuilder WithRole(string role)
    {
        _role = role;
        return this;
    }

    public UpdateUserDtoBuilder WithInvalidRole()
    {
        _role = "RoleInexistente";
        return this;
    }

    public UpdateUserDtoBuilder WithoutRole()
    {
        _role = null;
        return this;
    }

    public UpdateUserDtoBuilder WithoutName()
    {
        _name = null;
        return this;
    }

    public UpdateUserDtoBuilder WithoutEmail()
    {
        _email = null;
        return this;
    }

    public UpdateUserDtoBuilder WithoutPassword()
    {
        _password = null;
        return this;
    }

    public UpdateUserDto Build()
    {
        return new UpdateUserDto
        {
            Name = _name,
            Email = _email,
            Password = _password,
            Role = _role
        };
    }
}