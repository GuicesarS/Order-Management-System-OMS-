using OrderManagement.Application.Common;
using OrderManagement.Communication.Dtos.Auth;
using OrderManagement.Communication.Responses;

namespace OrderManagement.Application.Interfaces;
public interface IAuthService
{
    Task<Result<AuthResponse>> Login(LoginDto login);
}
