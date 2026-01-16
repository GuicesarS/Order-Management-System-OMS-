using OrderManagement.Application.Common;
using OrderManagement.Application.Common.CustomMapping;
using OrderManagement.Application.Exceptions;
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

    public UserService(IUserRepository repository, ICustomMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<UserResponse>> Create(CreateUserDto userDto)
    {
        var user = new User(
            userDto.Name,
            Email.Create(userDto.Email),
            Enum.Parse<UserRole>(userDto.Role)
        );

        await _repository.AddAsync(user);

        var response = _mapper.Map<User, UserResponse>(user);
        return Result<UserResponse>.Ok(response);

    }

    public async Task<Result<UserResponse>> Update(Guid id, UpdateUserDto updatedUser)
    {
        var existingUser = await _repository.GetUserByIdAsync(id);

        if (existingUser is null)
            throw new NotFoundException($"User with {id} was not found.");

        existingUser.UpdateUser(
            updatedUser.Name,
            Email.Create(updatedUser.Email),
            Enum.Parse<UserRole>(updatedUser.Role, ignoreCase: true)
        );
       
        await _repository.UpdateAsync(existingUser);

        var response = _mapper.Map<User, UserResponse>(existingUser);
        return Result<UserResponse>.Ok(response);
    }

    public async Task<Result<UserResponse>> GetUserById(Guid id)
    {
        var user = await _repository.GetUserByIdAsync(id);

        if (user is null)
            throw new NotFoundException($"User with {id} was not found.");

        var response = _mapper.Map<User, UserResponse>(user);
        return Result<UserResponse>.Ok(response);
    }

    public async Task<Result<IEnumerable<UserResponse>>> GetAll()
    {
        var users = await _repository.GetAllAsync();
        var userResponses = _mapper.Map<IEnumerable<User>, IEnumerable<UserResponse>>(users);

        return Result<IEnumerable<UserResponse>>.Ok(userResponses);
    }

    public async Task<Result<bool>> Delete(Guid id)
    {
        var user = await _repository.GetUserByIdAsync(id);

        if (user is null)
            throw new NotFoundException($"User with {id} was not found.");

        await _repository.DeleteAsync(id);

        return Result<bool>.Ok(true);
    }
}