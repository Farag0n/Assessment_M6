using Assessment_M6.Application.DTOs;
using Assessment_M6.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Assessment_M6.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    // POST: api/Auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var (accessToken, refreshToken) = await _userService.AuthenticateAsync(loginDto);
            
            return Ok(new 
            { 
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Message = "Login exitoso" 
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
    }

    // POST: api/Auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            var (accessToken, refreshToken) = await _userService.RegisterAsync(registerDto);
            
            return Ok(new 
            { 
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Message = "Registro exitoso" 
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // 🔥 NUEVO: POST: api/Auth/refresh
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto refreshTokenDto)
    {
        try
        {
            var (newAccessToken, newRefreshToken) = await _userService.RefreshTokenAsync(
                refreshTokenDto.AccessToken, 
                refreshTokenDto.RefreshToken
            );
            
            return Ok(new 
            { 
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Message = "Tokens refrescados exitosamente" 
            });
        }
        catch (SecurityTokenException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
    }
}