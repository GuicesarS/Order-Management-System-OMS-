using FluentAssertions;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Exception;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Tests.Domain.Builders;

namespace OrderManagement.Tests.Domain.Entities;
public class UserTest
{
    // Constructor Tests

    [Fact]
    public void Constructor_ShouldCreateUser_WhenValidData()
    {
        var user = new UserBuilder().Build();

        user.Id.Should().NotBeEmpty();
        user.Name.Should().NotBeNullOrWhiteSpace();
        user.Email.Should().NotBeNull();
        Enum.IsDefined(typeof(UserRole), user.Role).Should().BeTrue();

    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameIsEmpty()
    {
        var user = new UserBuilder();

        Action act = () => user.BuildInvalidUserWithEmptyName();

        act.Should().
            Throw<DomainValidationException>()
           .WithMessage("Name cannot be empty.");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenEmailIsNull()
    {
        var user = new UserBuilder();

        Action act = () => user.BuildInvalidUserWithNullEmail();

        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage("Email cannot be null.");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenRoleIsInvalid()
    {
        var user = new UserBuilder();

        Action act = () => user.BuildWithInvalidRole();

        act.Should()
            .Throw<DomainValidationException>();
    }

    //Update Name Test

    [Fact]
    public void UpdateName_ShouldBeSuccessful_WhenValidData()
    {
        var user = new UserBuilder().Build();

        var updateName = "New name";

        user.UpdateName(updateName);

        user.Should().NotBeNull();
        user.Name.Should().Be(updateName);
    }

    [Fact]
    public void UpdateName_ShouldThrowException_WhenNameIsEmpty()
    {
        var user = new UserBuilder().Build();

        Action act = () => user.UpdateName("");

        act.Should()
       .Throw<DomainValidationException>()
       .WithMessage("Name cannot be empty.");
    }

    // Update Email Test

    [Fact]
    public void UpdateEmail_ShouldBeSuccessful_WhenValidData()
    {
        var user = new UserBuilder().Build();

        var newEmail = Email.Create("newemail@gmail.com");

        user.UpdateEmail(newEmail);

        user.Should().NotBeNull();
        user.Email.Should().Be(newEmail);
        user.Email.Value.Should().Be("newemail@gmail.com");
    }

    [Fact]
    public void UpdateEmail_ShouldThrowException_WhenEmailIsInvalid()
    {
        var user = new UserBuilder().Build();

        Action act = () => user.UpdateEmail(Email.Create("invalid-email"));

        act.Should().Throw<DomainValidationException>()
            .WithMessage("Email format is invalid.");
    }

    [Fact]
    public void UpdateEmail_ShouldThrowException_WhenEmailIsNull()
    {
        var user = new UserBuilder().Build();

        Action act = () => user.UpdateEmail(Email.Create(null!));

        act.Should().Throw<DomainValidationException>()
            .WithMessage("Email is required.");
    }

    // Update Role Test

    [Fact]
    public void UpdateRole_ShouldBeSuccessful_WhenValidData()
    {
        var user = new UserBuilder().Build();
        var newRole = UserRole.Operator;

        user.UpdateRole(newRole);

        user.Role.Should().Be(newRole);
    }

    [Fact]
    public void UpdateRole_ShouldThrowException_WhenRoleIsInvalid()
    {
        var user = new UserBuilder().Build();

        Action act = () => user.UpdateRole((UserRole)999);

        act.Should().Throw<DomainValidationException>();
        
    }

    // Update User Test

    [Fact]
    public void UpdateUser_ShouldBeSuccessful_WhenValidData()
    {
        var user = new UserBuilder().Build();

        var newName = "João Pereira";
        var newEmail = Email.Create("joao@gmail.com");
        var newRole = UserRole.Operator;

        user.UpdateUser(newName, newEmail, newRole);

        user.Should().NotBeNull();
        user.Name.Should().Be(newName);
    }

    [Theory]
    [InlineData("", "teste@gmail.com", UserRole.Admin, "Name cannot be empty.")]
    [InlineData("Guilherme", null, UserRole.Admin, "email")]
    [InlineData("Guilherme", "teste@gmail.com", (UserRole)999, "not valid")]
    public void UpdateUser_ShouldThrowException_WhenDataIsInvalid(
     string name,
     string? emailValue,
     UserRole role,
     string expectedMessage
 )
    {
        var user = new UserBuilder().Build();

        Email? email = emailValue is null 
            ? null
            : Email.Create(emailValue);

        Action act = () => user.UpdateUser(name, email!, role);

        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage($"*{expectedMessage}*");
    }
}
