using Assessment_M6.Application.DTOs;

namespace Assessment_M6.Application.Interfaces;

public interface IUserService
{
    Task<UserDtos.UserResponseDTO?> GetUserByIdAsync(int id);
    Task<UserDtos.UserResponseDTO?> GetUserByEmailAsync(string email);
    Task<IEnumerable<UserDtos.UserResponseDTO>> GetAllUsersAsync();
    Task<UserDtos.UserResponseDTO> CreateUserAsync(UserDtos.UserCreateDTO userCreateDto);
    Task<UserDtos.UserResponseDTO?> UpdateUserAsync(UserDtos.UserUpdateDTO userUpdateDto);
    Task<UserDtos.UserResponseDTO?> DeleteUserAsync(int id);
    
    //retorna AccessToken y RefreshToken
    Task<(string AccessToken, string RefreshToken)> AuthenticateAsync(LoginDto loginDto);
    Task<(string AccessToken, string RefreshToken)> RegisterAsync(RegisterDto registerDto);
    
    //Metodo para refrescar el token
    Task<(string NewAccessToken, string NewRefreshToken)> RefreshTokenAsync(string accessToken, string refreshToken);
}