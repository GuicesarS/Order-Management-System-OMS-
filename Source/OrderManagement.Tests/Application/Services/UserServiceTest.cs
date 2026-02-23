using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Application.Common.CustomMapping;
using OrderManagement.Application.Exceptions;
using OrderManagement.Application.Services;
using OrderManagement.Communication.Dtos.User;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.User;
using OrderManagement.Domain.Exception;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Domain.Security.Cryptography;
using OrderManagement.Tests.Application.Builders;
using OrderManagement.Tests.Application.ExtensionMethods;
using OrderManagement.Tests.Domain.Builders;

namespace OrderManagement.Tests.Application.Services;

public class UserServiceTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICustomMapper> _mapperMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;

    private readonly UserService _userService;

    public UserServiceTest()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<ICustomMapper>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _passwordHasherMock = new Mock<IPasswordHasher>();

        _userService = new UserService(
            _userRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _passwordHasherMock.Object
        );
    }

    [Fact]
    public async Task CreateUser_ShouldBeSuccessful()
    {
        var userDto = new CreateUserDtoBuilder().Build();
        var hashedPassword = "hashed-password";

        _passwordHasherMock
            .Setup(p => p.Hash(userDto.Password))
            .Returns(hashedPassword);

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(m => m.Map<User, UserResponse>(It.IsAny<User>()))
            .Returns((User user) => new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email.Value,
                Role = user.Role.ToString()
            });

        var result = await _userService.Create(userDto);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(userDto.Name);
        result.Data.Email.Should().Be(userDto.Email);
        result.Data.Id.Should().NotBe(Guid.Empty);

        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _passwordHasherMock.Verify(p => p.Hash(userDto.Password), Times.Once);
        _loggerMock.VerifyLog($"User {result.Data.Id} created successfully with role: {result.Data.Role}");
    }

    [Fact]
    public async Task CreateUser_ShouldThrowException_WhenEmailIsInvalid()
    {
        var userDto = new CreateUserDtoBuilder()
            .WithInvalidEmail()
            .Build();

        Func<Task> act = async () => await _userService.Create(userDto);

        await act.Should().ThrowAsync<DomainValidationException>()
            .WithMessage("Email format is invalid.");
    }

    [Fact]
    public async Task CreateUser_ShouldThrowException_WhenRoleIsInvalid()
    {
        var userDto = new CreateUserDtoBuilder()
            .WithInvalidRole()
            .Build();

        Func<Task> act = async () => await _userService.Create(userDto);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateUser_ShouldBeSuccessful()
    {
        var existingUser = new UserBuilder().Build();
        var updateDto = new UpdateUserDtoBuilder().Build();
        var hashedPassword = "new-hashed-password";

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(existingUser.Id))
            .ReturnsAsync(existingUser);

        _passwordHasherMock
            .Setup(p => p.Hash(updateDto.Password!))
            .Returns(hashedPassword);

        _userRepositoryMock
            .Setup(r => r.UpdateAsync(existingUser))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(m => m.Map<User, UserResponse>(existingUser))
            .Returns(new UserResponse
            {
                Id = existingUser.Id,
                Name = existingUser.Name,
                Email = existingUser.Email.Value,
                Role = existingUser.Role.ToString()
            });

        var result = await _userService.Update(existingUser.Id, updateDto);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(existingUser.Id);

        _userRepositoryMock.Verify(r => r.UpdateAsync(existingUser), Times.Once);
        _loggerMock.VerifyLog($"User {existingUser.Id} updated successfully");
    }

    [Fact]
    public async Task UpdateUser_ShouldThrowNotFoundException_WhenUserNotFound()
    {
        var invalidId = Guid.NewGuid();
        var updateDto = new UpdateUserDtoBuilder().Build();

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(invalidId))
            .ReturnsAsync((User?)null);

        Func<Task> act = async () => await _userService.Update(invalidId, updateDto);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"User with {invalidId} was not found.");
    }

    [Fact]
    public async Task UpdateUser_ShouldThrowValidationException_WhenRoleIsInvalid()
    {
        var existingUser = new UserBuilder().Build();

        var updateDto = new UpdateUserDtoBuilder()
            .WithInvalidRole()
            .Build();

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(existingUser.Id))
            .ReturnsAsync(existingUser);

        _passwordHasherMock
            .Setup(p => p.Hash(updateDto.Password!))
            .Returns("hashed-password");

        Func<Task> act = async () => await _userService.Update(existingUser.Id, updateDto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Invalid role.");
    }

    [Fact]
    public async Task UpdateUser_ShouldNotHashPassword_WhenPasswordIsNotProvided()
    {
        var existingUser = new UserBuilder().Build();

        var updateDto = new UpdateUserDtoBuilder()
            .WithoutPassword()
            .Build();

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(existingUser.Id))
            .ReturnsAsync(existingUser);

        _userRepositoryMock
            .Setup(r => r.UpdateAsync(existingUser))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(m => m.Map<User, UserResponse>(existingUser))
            .Returns(new UserResponse
            {
                Id = existingUser.Id,
                Name = existingUser.Name,
                Email = existingUser.Email.Value,
                Role = existingUser.Role.ToString()
            });

        await _userService.Update(existingUser.Id, updateDto);

        _passwordHasherMock.Verify(p => p.Hash(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUser_ShouldBeSuccessful()
    {
        var existingUser = new UserBuilder().Build();

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(existingUser.Id))
            .ReturnsAsync(existingUser);

        _userRepositoryMock
            .Setup(r => r.DeleteAsync(existingUser.Id))
            .Returns(Task.CompletedTask);

        var result = await _userService.Delete(existingUser.Id);

        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();

        _userRepositoryMock.Verify(r => r.DeleteAsync(existingUser.Id), Times.Once);
        _loggerMock.VerifyLog($"Attempting to delete user {existingUser.Id}");
        _loggerMock.VerifyLog($"User {existingUser.Id} deleted successfully");
    }

    [Fact]
    public async Task DeleteUser_ShouldThrowNotFoundException_WhenUserNotFound()
    {
        var invalidId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(invalidId))
            .ReturnsAsync((User?)null);

        Func<Task> act = async () => await _userService.Delete(invalidId);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"User with {invalidId} was not found.");
    }

    [Fact]
    public async Task GetAllUsers_ShouldBeSuccessful()
    {
        var users = new List<User>
        {
            new UserBuilder().Build(),
            new UserBuilder().Build()
        };

        _userRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<User>, IEnumerable<UserResponse>>(It.IsAny<IEnumerable<User>>()))
            .Returns((IEnumerable<User> u) => u.Select(user => new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email.Value,
                Role = user.Role.ToString()
            }));

        var result = await _userService.GetAll();

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();
        result.Data!.Count().Should().Be(2);

        _loggerMock.VerifyLog($"Retrieved {result.Data!.Count()} users");
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        _userRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());

        _mapperMock
            .Setup(m => m.Map<IEnumerable<User>, IEnumerable<UserResponse>>(It.IsAny<IEnumerable<User>>()))
            .Returns(Enumerable.Empty<UserResponse>());

        var result = await _userService.GetAll();

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserById_ShouldBeSuccessful()
    {
        var existingUser = new UserBuilder().Build();

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(existingUser.Id))
            .ReturnsAsync(existingUser);

        _mapperMock
            .Setup(m => m.Map<User, UserResponse>(It.IsAny<User>()))
            .Returns((User user) => new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email.Value,
                Role = user.Role.ToString()
            });

        var result = await _userService.GetUserById(existingUser.Id);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(existingUser.Id);
        result.Data.Name.Should().Be(existingUser.Name);

        _loggerMock.VerifyLog($"User {existingUser.Id} retrieved successfully");
    }

    [Fact]
    public async Task GetUserById_ShouldThrowNotFoundException_WhenUserNotFound()
    {
        var invalidId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(invalidId))
            .ReturnsAsync((User?)null);

        Func<Task> act = async () => await _userService.GetUserById(invalidId);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"User with {invalidId} was not found.");
    }
}