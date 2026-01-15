using OrderManagement.Application.Common;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.User;

namespace OrderManagement.Application.Interfaces;

public interface IUserService
{
    Task<Result<UserResponse>> Create(User user);
    Task<Result<UserResponse>> Update(Guid id, User user);
    Task<Result<UserResponse>> GetUserById(Guid id);
    Task<Result<IEnumerable<UserResponse>>> GetAll();
    Task<Result<bool>> Delete(Guid id);
}
