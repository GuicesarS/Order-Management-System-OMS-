using OrderManagement.Application.Common;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.User;

namespace OrderManagement.Application.Interfaces;

public interface IUserService
{
    Result<UserResponse> Create(User user);
    Result<UserResponse> Update(Guid id, User user);
    Result<UserResponse> GetUserById(Guid id);
    Result<IEnumerable<UserResponse>> GetAll();
    Result<bool> Delete(Guid id);
}
