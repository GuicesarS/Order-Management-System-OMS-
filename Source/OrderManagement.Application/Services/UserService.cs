using OrderManagement.Application.Common;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Validators.User;
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
    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }
    public Result<UserResponse> Create(CreateUserDto createUserDto)
    {
        var validator = new CreateUserDtoValidator();
        var validationResult = validator.Validate(createUserDto);

        if (!validationResult.IsValid)
            return Result<UserResponse>.Failure(validationResult.Errors.First().ErrorMessage);

        var emailVo = Email.Create(createUserDto.Email);
        var roleEnum = Enum.Parse<UserRole>(createUserDto.Role, ignoreCase: true);

        var user = new User(createUserDto.Name, emailVo, roleEnum);

        _repository.Add(user);

        return Result<UserResponse>.Ok(MapToResponse(user));
    }
    public Result<UserResponse> Update(Guid id, UpdateUserDto updateUserDto)
    {
        var validator = new UpdateUserDtoValidator();
        var validatorResult = validator.Validate(updateUserDto);

        if (!validatorResult.IsValid)
            return Result<UserResponse>.Failure(validatorResult.Errors.First().ErrorMessage);

        var user = _repository.GetUserById(id);

        if (user is null)
            return Result<UserResponse>.Failure("User not found.");

        if (!string.IsNullOrWhiteSpace(updateUserDto.Name))
            user.UpdateName(updateUserDto.Name);

        if (!string.IsNullOrWhiteSpace(updateUserDto.Email))
            user.UpdateEmail(Email.Create(updateUserDto.Email));

        if (!string.IsNullOrWhiteSpace(updateUserDto.Role))
            user.UpdateRole(Enum.Parse<UserRole>(updateUserDto.Role, ignoreCase: true));

        _repository.Update(user);

        return Result<UserResponse>.Ok(MapToResponse(user));

    }

    public Result<UserResponse> GetById(Guid id)
    {
        var user = _repository.GetUserById(id);

        if (user is null)
            return Result<UserResponse>.Failure("User not found.");

        return Result<UserResponse>.Ok(MapToResponse(user));
    }

    public Result<IEnumerable<UserResponse>> GetAll()
    {
        var users = _repository.GetAll();
        var userResponses = users.Select(MapToResponse);

        return Result<IEnumerable<UserResponse>>.Ok(userResponses);
    }
    public Result<bool> Delete(Guid id)
    {
        var user = _repository.GetUserById(id);

        if (user is null)
            return Result<bool>.Failure("User not found.");

        _repository.Delete(id);

        return Result<bool>.Ok(true);
    }

    private static UserResponse MapToResponse(User user) =>
            new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email.ToString(),
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            };

}
