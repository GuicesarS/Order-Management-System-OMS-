using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Interfaces;
using OrderManagement.Communication.Dtos.Auth;

namespace OrderManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto login)
    {
        var response = await _authService.Login(login);

        return Ok(response);
    }
}
