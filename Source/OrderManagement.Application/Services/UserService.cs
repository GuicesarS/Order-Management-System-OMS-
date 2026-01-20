using OrderManagement.Application.Common;
using OrderManagement.Application.Common.CustomMapping;
using OrderManagement.Application.Exceptions;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Interfaces;
using OrderManagement.Communication.Dtos.User;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.User;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ICustomMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository repository, ICustomMapper mapper, ILogger<UserService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<UserResponse>> Create(CreateUserDto userDto)
    {
        _logger.LogInformation("Creating a new user with email: {Email}", userDto.Email);

        var user = new User(
            userDto.Name,
            Email.Create(userDto.Email),
            Enum.Parse<UserRole>(userDto.Role)
        );

        await _repository.AddAsync(user);

        _logger.LogInformation("User {UserId} created successfully with role: {Role}", user.Id, user.Role);

        var response = _mapper.Map<User, UserResponse>(user);
        return Result<UserResponse>.Ok(response);

    }
    public async Task<Result<UserResponse>> Update(Guid id, UpdateUserDto updatedUser)
    {
        _logger.LogInformation("Updating user {UserId}", id);

        var existingUser = await _repository.GetUserByIdAsync(id);

        if (existingUser is null)
        {
            _logger.LogWarning("User {UserId} not found for update", id);
            throw new NotFoundException($"User with {id} was not found.");
        }
       
        existingUser.UpdateUser(
            updatedUser.Name,
            Email.Create(updatedUser.Email),
            Enum.Parse<UserRole>(updatedUser.Role, ignoreCase: true)
        );
       
        await _repository.UpdateAsync(existingUser);

        _logger.LogInformation("User {UserId} updated successfully", id);

        var response = _mapper.Map<User, UserResponse>(existingUser);
        return Result<UserResponse>.Ok(response);
    }
    public async Task<Result<UserResponse>> GetUserById(Guid id)
    {
        _logger.LogInformation("Fetching user {UserId}", id);

        var user = await _repository.GetUserByIdAsync(id);

        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found", id);
            throw new NotFoundException($"User with {id} was not found.");
        }

        _logger.LogInformation("User {UserId} retrieved successfully", id);

        var response = _mapper.Map<User, UserResponse>(user);
        return Result<UserResponse>.Ok(response);
    }
    public async Task<Result<IEnumerable<UserResponse>>> GetAll()
    {
        _logger.LogInformation("Fetching all users");

        var users = await _repository.GetAllAsync();
        var userResponses = _mapper.Map<IEnumerable<User>, IEnumerable<UserResponse>>(users);

        _logger.LogInformation("Retrieved {UserCount} users", userResponses.Count());

        return Result<IEnumerable<UserResponse>>.Ok(userResponses);
    }
    public async Task<Result<bool>> Delete(Guid id)
    {
        _logger.LogInformation("Attempting to delete user {UserId}", id);

        var user = await _repository.GetUserByIdAsync(id);

        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found for deletion", id);
            throw new NotFoundException($"User with {id} was not found.");
        }

        await _repository.DeleteAsync(id);

        _logger.LogInformation("User {UserId} deleted successfully", id);

        return Result<bool>.Ok(true);
    }
}