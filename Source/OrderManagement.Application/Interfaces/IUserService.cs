using OrderManagement.Application.Common;
using OrderManagement.Communication.Dtos.User;
using OrderManagement.Communication.Responses;

namespace OrderManagement.Application.Interfaces;

public interface IUserService
{
    Result<UserResponse> Create(CreateUserDto createUserDto);
    Result<UserResponse> Update(Guid id, UpdateUserDto updateUserDto);
    Result<UserResponse> GetById(Guid id);
    Result<IEnumerable<UserResponse>> GetAll();
    Result<bool> Delete(Guid id);
}
