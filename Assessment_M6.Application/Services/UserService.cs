using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Assessment_M6.Application.DTOs;
using Assessment_M6.Application.Interfaces;
using Assessment_M6.Application.Services;
using Assessment_M6.Domain.Entities;
using Assessment_M6.Domain.Enums;
using Assessment_M6.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace EduTrack.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly TokenService _tokenService;

    public UserService(IUserRepository userRepository, TokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<UserDtos.UserResponseDTO?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetUserById(id);
        return user == null ? null : MapToUserResponseDto(user);
    }

    public async Task<UserDtos.UserResponseDTO?> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetUserByEmail(email);
        return user == null ? null : MapToUserResponseDto(user);
    }

    public async Task<IEnumerable<UserDtos.UserResponseDTO>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllUsers();
        return users.Select(MapToUserResponseDto);
    }

    public async Task<UserDtos.UserResponseDTO> CreateUserAsync(UserDtos.UserCreateDTO userCreateDto)
    {
        var existingUser = await _userRepository.GetUserByEmail(userCreateDto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("El email ya está registrado");
        
        var existingByUsername = (await _userRepository.GetAllUsers())
            .FirstOrDefault(u => u.Username == userCreateDto.Username);
        if (existingByUsername != null)
            throw new InvalidOperationException("El nombre de usuario ya está en uso");

        var user = new User
        {
            Email = userCreateDto.Email,
            Username = userCreateDto.Username,
            PasswordHash = HashPassword(userCreateDto.Password),
            Role = userCreateDto.Role
        };

        var createdUser = await _userRepository.AddUser(user);
        return MapToUserResponseDto(createdUser);
    }

    public async Task<UserDtos.UserResponseDTO?> UpdateUserAsync(UserDtos.UserUpdateDTO userUpdateDto)
    {
        var existingUser = await _userRepository.GetUserById(userUpdateDto.Id);
        if (existingUser == null)
            return null;
        
        if (existingUser.Email != userUpdateDto.Email)
        {
            var userWithSameEmail = await _userRepository.GetUserByEmail(userUpdateDto.Email);
            if (userWithSameEmail != null && userWithSameEmail.Id != userUpdateDto.Id)
                throw new InvalidOperationException("El email ya está registrado por otro usuario");
        }
        
        if (existingUser.Username != userUpdateDto.Username)
        {
            var allUsers = await _userRepository.GetAllUsers();
            var userWithSameUsername = allUsers.FirstOrDefault(u => 
                u.Username == userUpdateDto.Username && u.Id != userUpdateDto.Id);
            if (userWithSameUsername != null)
                throw new InvalidOperationException("El nombre de usuario ya está en uso");
        }

        existingUser.Email = userUpdateDto.Email;
        existingUser.Username = userUpdateDto.Username;
        
        if (!string.IsNullOrEmpty(userUpdateDto.Password))
        {
            existingUser.PasswordHash = HashPassword(userUpdateDto.Password);
        }
        
        existingUser.Role = userUpdateDto.Role;

        var updatedUser = await _userRepository.UpdateUser(existingUser);
        return updatedUser == null ? null : MapToUserResponseDto(updatedUser);
    }

    public async Task<UserDtos.UserResponseDTO?> DeleteUserAsync(int id)
    {
        var deletedUser = await _userRepository.DeleteUser(id);
        return deletedUser == null ? null : MapToUserResponseDto(deletedUser);
    }

    public async Task<(string AccessToken, string RefreshToken)> AuthenticateAsync(LoginDto loginDto)
    {
        if (string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
            throw new ArgumentException("Email y contraseña son requeridos");

        var user = await _userRepository.GetUserByEmail(loginDto.Email);
        if (user == null || user.PasswordHash != HashPassword(loginDto.Password))
            throw new UnauthorizedAccessException("Credenciales inválidas");

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());
        var refreshToken = _tokenService.GenerateRefreshToken();
        
        var refreshTokenExpirationDays = _tokenService.GetRefreshTokenExpirationDays();
        
        // Save refresh token in database
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(refreshTokenExpirationDays);
        await _userRepository.UpdateUser(user);

        return (accessToken, refreshToken);
    }

    public async Task<(string AccessToken, string RefreshToken)> RegisterAsync(RegisterDto registerDto)
    {
        var existingUser = await _userRepository.GetUserByEmail(registerDto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("El usuario ya existe");

        var user = new User
        {
            Email = registerDto.Email,
            Username = registerDto.Username,
            PasswordHash = HashPassword(registerDto.Password),
            Role = registerDto.Role
        };

        await _userRepository.AddUser(user);
        
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());
        var refreshToken = _tokenService.GenerateRefreshToken();
        
        var refreshTokenExpirationDays = _tokenService.GetRefreshTokenExpirationDays();
        
        //Save Token
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(refreshTokenExpirationDays);
        await _userRepository.UpdateUser(user);

        return (accessToken, refreshToken);
    }

    public async Task<(string NewAccessToken, string NewRefreshToken)> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        //Get user to expired Token
        var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim))
            throw new SecurityTokenException("Token inválido");

        var userId = int.Parse(userIdClaim);
        var user = await _userRepository.GetUserById(userId);
        
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryDate <= DateTime.UtcNow)
            throw new SecurityTokenException("Refresh token inválido o expirado");

        // Generate new Token
        var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpirationDays = _tokenService.GetRefreshTokenExpirationDays();
        
        // Update refresh token in db
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(refreshTokenExpirationDays);
        await _userRepository.UpdateUser(user);

        return (newAccessToken, newRefreshToken);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
    }
    
    private UserDtos.UserResponseDTO MapToUserResponseDto(User user)
    {
        return new UserDtos.UserResponseDTO
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Role = user.Role
        };
    }
}