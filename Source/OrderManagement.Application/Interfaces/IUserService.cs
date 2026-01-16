using OrderManagement.Application.Common;
using OrderManagement.Communication.Dtos.User;
using OrderManagement.Communication.Responses;

namespace OrderManagement.Application.Interfaces;

public interface IUserService
{
    Task<Result<UserResponse>> Create(CreateUserDto userDto);
    Task<Result<UserResponse>> Update(Guid id, UpdateUserDto userDto);
    Task<Result<UserResponse>> GetUserById(Guid id);
    Task<Result<IEnumerable<UserResponse>>> GetAll();
    Task<Result<bool>> Delete(Guid id);
}
