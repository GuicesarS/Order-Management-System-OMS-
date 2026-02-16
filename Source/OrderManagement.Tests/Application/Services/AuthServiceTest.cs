using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Application.Exceptions;
using OrderManagement.Application.Services;
using OrderManagement.Communication.Dtos.Auth;
using OrderManagement.Domain.Entities.User;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Domain.Security.Cryptography;
using OrderManagement.Domain.Security.Token;
using OrderManagement.Tests.Domain.Builders;

namespace OrderManagement.Tests.Application.Services;

public class AuthServiceTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IAccessTokenGenerator> _accessTokenMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;

    private readonly AuthService _authService;

    public AuthServiceTest()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _accessTokenMock = new Mock<IAccessTokenGenerator>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _accessTokenMock.Object,
            _passwordHasherMock.Object,
            _loggerMock.Object
            );
    }

    [Fact]
    public async Task Login_ShouldReturnToken_Success()
    {
        var login = new LoginDto
        {
            Email = "test@gmail.com",
            Password = "123456"
        };

        var user = new UserBuilder().Build();

        _userRepositoryMock
            .Setup(x => x.GetUserByEmailAsync(login.Email))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(login.Password, user.PasswordHash))
            .Returns(true);

        _accessTokenMock
            .Setup(x => x.Generate(user))
            .Returns("fake-jwt-token");

        var result = await _authService.Login(login);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("fake-jwt-token");

        _accessTokenMock.Verify(x => x.Generate(user), Times.Once);
    }

    [Fact]
    public async Task Login_ShouldThrowUnauthorizedException_WhenUserNotFound()
    {
        var login = new LoginDto
        {
            Email = "notfound@gmail.com",
            Password = "123456"
        };

        _userRepositoryMock
            .Setup(x => x.GetUserByEmailAsync(login.Email))
            .ReturnsAsync((User?)null);

        Func<Task> act = async () => await _authService.Login(login);
        
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials.");

        _passwordHasherMock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _accessTokenMock.Verify(x => x.Generate(It.IsAny<User>()), Times.Never);

    }

    [Fact]
    public async Task Login_ShouldThrowUnauthorizedException_WhenPasswordIsInvalid()
    {
        var login = new LoginDto
        {
            Email = "test@gmail.com",
            Password = "wrongpassword"
        };

        var user = new UserBuilder().Build();

        _userRepositoryMock
            .Setup(x => x.GetUserByEmailAsync(login.Email))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(login.Password, user.PasswordHash))
            .Returns(false);

        Func<Task> act = async () => await _authService.Login(login);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials.");

        _passwordHasherMock.Verify(x => x.Verify(login.Password, user.PasswordHash), Times.Once);
        _accessTokenMock.Verify(x => x.Generate(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Login_ShouldLogInformation_WhenAuthenticating()
    {
        var login = new LoginDto
        {
            Email = "test@gmail.com",
            Password = "123456"
        };

        var user = new UserBuilder().Build();

        _userRepositoryMock
            .Setup(x => x.GetUserByEmailAsync(login.Email))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(login.Password, user.PasswordHash))
            .Returns(true);

        _accessTokenMock
            .Setup(x => x.Generate(user))
            .Returns("fake-jwt-token");

        await _authService.Login(login);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Authenticating user with email {login.Email}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

}
