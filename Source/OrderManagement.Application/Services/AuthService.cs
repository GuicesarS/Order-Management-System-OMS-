using Microsoft.Extensions.Logging;
using OrderManagement.Application.Common;
using OrderManagement.Application.Exceptions;
using OrderManagement.Application.Interfaces;
using OrderManagement.Communication.Dtos.Auth;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Domain.Security.Cryptography;
using OrderManagement.Domain.Security.Token;

namespace OrderManagement.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IAccessTokenGenerator _accessToken;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository, 
        IAccessTokenGenerator accessToken, 
        IPasswordHasher passwordHasher, 
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _accessToken = accessToken;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Login(LoginDto login)
    {
        _logger.LogInformation("Authenticating user with email {Email}", login.Email);

        var user = await _userRepository.GetUserByEmailAsync(login.Email);

        if (user is null)
            throw new UnauthorizedException("Invalid credentials.");

        var validPassword = _passwordHasher.Verify(login.Password, user.PasswordHash);

        if (!validPassword)
            throw new UnauthorizedException("Invalid credentials.");

        var token = _accessToken.Generate(user);

        return Result<AuthResponse>.Ok(new AuthResponse
        {
            AccessToken = token
        });
    }
}
