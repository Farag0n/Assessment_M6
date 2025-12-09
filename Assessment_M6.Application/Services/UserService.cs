using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Assessment_M6.Application.DTOs;
using Assessment_M6.Application.Interfaces;
using Assessment_M6.Application.Services;
using Assessment_M6.Domain.Entities;
using Assessment_M6.Domain.Enums;
using Assessment_M6.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Assessment_M6.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly TokenService _tokenService;
    private readonly ILogger<UserService> _logger; // <-- CORREGIDO

    public UserService(
        IUserRepository userRepository, 
        TokenService tokenService, 
        ILogger<UserService> logger) // <-- CORREGIDO
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<UserDtos.UserResponseDTO?> GetUserByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Obteniendo usuario por ID: {Id}", id);
            
            var user = await _userRepository.GetUserById(id);
            
            if (user == null)
            {
                _logger.LogWarning("Usuario con ID {Id} no encontrado", id);
            }
            else
            {
                _logger.LogInformation("Usuario con ID {Id} encontrado: {Email}", id, user.Email);
            }
            
            return user == null ? null : MapToUserResponseDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario por ID {Id}", id);
            throw;
        }
    }

    public async Task<UserDtos.UserResponseDTO?> GetUserByEmailAsync(string email)
    {
        try
        {
            _logger.LogInformation("Buscando usuario por email: {Email}", email);
            
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("El email es requerido");
            }
            
            var user = await _userRepository.GetUserByEmail(email);
            
            if (user == null)
            {
                _logger.LogWarning("Usuario con email {Email} no encontrado en el repositorio", email);
            }
            else
            {
                _logger.LogInformation("Usuario con email {Email} encontrado: ID={Id}", email, user.Id);
            }
            
            return user == null ? null : MapToUserResponseDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar usuario por email {Email}", email);
            throw;
        }
    }

    public async Task<IEnumerable<UserDtos.UserResponseDTO>> GetAllUsersAsync()
    {
        try
        {
            _logger.LogInformation("Obteniendo todos los usuarios");
            
            var users = await _userRepository.GetAllUsers();
            
            _logger.LogInformation("Se encontraron {Count} usuarios", users.Count());
            
            return users.Select(MapToUserResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los usuarios");
            throw;
        }
    }

    public async Task<UserDtos.UserResponseDTO> CreateUserAsync(UserDtos.UserCreateDTO userCreateDto)
    {
        try
        {
            _logger.LogInformation("Creando usuario: {Email}", userCreateDto.Email);
            
            var existingUser = await _userRepository.GetUserByEmail(userCreateDto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("El email {Email} ya está registrado", userCreateDto.Email);
                throw new InvalidOperationException("El email ya está registrado");
            }
            
            var existingByUsername = (await _userRepository.GetAllUsers())
                .FirstOrDefault(u => u.Username == userCreateDto.Username);
            if (existingByUsername != null)
            {
                _logger.LogWarning("El nombre de usuario {Username} ya está en uso", userCreateDto.Username);
                throw new InvalidOperationException("El nombre de usuario ya está en uso");
            }

            var user = new User
            {
                Email = userCreateDto.Email,
                Username = userCreateDto.Username,
                PasswordHash = HashPassword(userCreateDto.Password),
                Role = userCreateDto.Role
            };

            var createdUser = await _userRepository.AddUser(user);
            
            _logger.LogInformation("Usuario creado exitosamente: ID={Id}", createdUser.Id);
            
            return MapToUserResponseDto(createdUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario");
            throw;
        }
    }

    public async Task<UserDtos.UserResponseDTO?> UpdateUserAsync(UserDtos.UserUpdateDTO userUpdateDto)
    {
        try
        {
            _logger.LogInformation("Actualizando usuario ID: {Id}", userUpdateDto.Id);
            
            var existingUser = await _userRepository.GetUserById(userUpdateDto.Id);
            if (existingUser == null)
            {
                _logger.LogWarning("Usuario con ID {Id} no encontrado para actualizar", userUpdateDto.Id);
                return null;
            }
            
            if (existingUser.Email != userUpdateDto.Email)
            {
                var userWithSameEmail = await _userRepository.GetUserByEmail(userUpdateDto.Email);
                if (userWithSameEmail != null && userWithSameEmail.Id != userUpdateDto.Id)
                {
                    _logger.LogWarning("El email {Email} ya está registrado por otro usuario", userUpdateDto.Email);
                    throw new InvalidOperationException("El email ya está registrado por otro usuario");
                }
            }
            
            if (existingUser.Username != userUpdateDto.Username)
            {
                var allUsers = await _userRepository.GetAllUsers();
                var userWithSameUsername = allUsers.FirstOrDefault(u => 
                    u.Username == userUpdateDto.Username && u.Id != userUpdateDto.Id);
                if (userWithSameUsername != null)
                {
                    _logger.LogWarning("El nombre de usuario {Username} ya está en uso", userUpdateDto.Username);
                    throw new InvalidOperationException("El nombre de usuario ya está en uso");
                }
            }

            existingUser.Email = userUpdateDto.Email;
            existingUser.Username = userUpdateDto.Username;
            
            if (!string.IsNullOrEmpty(userUpdateDto.Password))
            {
                existingUser.PasswordHash = HashPassword(userUpdateDto.Password);
            }
            
            existingUser.Role = userUpdateDto.Role;

            var updatedUser = await _userRepository.UpdateUser(existingUser);
            
            if (updatedUser == null)
            {
                _logger.LogWarning("Error al actualizar usuario ID: {Id}", userUpdateDto.Id);
            }
            else
            {
                _logger.LogInformation("Usuario ID {Id} actualizado exitosamente", userUpdateDto.Id);
            }
            
            return updatedUser == null ? null : MapToUserResponseDto(updatedUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario ID {Id}", userUpdateDto.Id);
            throw;
        }
    }

    public async Task<UserDtos.UserResponseDTO?> DeleteUserAsync(int id)
    {
        try
        {
            _logger.LogInformation("Eliminando usuario ID: {Id}", id);
            
            var deletedUser = await _userRepository.DeleteUser(id);
            
            if (deletedUser == null)
            {
                _logger.LogWarning("Usuario con ID {Id} no encontrado para eliminar", id);
            }
            else
            {
                _logger.LogInformation("Usuario ID {Id} eliminado exitosamente", id);
            }
            
            return deletedUser == null ? null : MapToUserResponseDto(deletedUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario ID {Id}", id);
            throw;
        }
    }

    public async Task<(string AccessToken, string RefreshToken)> AuthenticateAsync(LoginDto loginDto)
    {
        try
        {
            _logger.LogInformation("Autenticando usuario: {Email}", loginDto.Email);
            
            if (string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
                throw new ArgumentException("Email y contraseña son requeridos");

            var user = await _userRepository.GetUserByEmail(loginDto.Email);
            if (user == null || user.PasswordHash != HashPassword(loginDto.Password))
            {
                _logger.LogWarning("Credenciales inválidas para {Email}", loginDto.Email);
                throw new UnauthorizedAccessException("Credenciales inválidas");
            }

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());
            var refreshToken = _tokenService.GenerateRefreshToken();
            
            var refreshTokenExpirationDays = _tokenService.GetRefreshTokenExpirationDays();
            
            // Save refresh token in database
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(refreshTokenExpirationDays);
            await _userRepository.UpdateUser(user);

            _logger.LogInformation("Usuario {Email} autenticado exitosamente", loginDto.Email);
            
            return (accessToken, refreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en autenticación para {Email}", loginDto.Email);
            throw;
        }
    }

    public async Task<(string AccessToken, string RefreshToken)> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            _logger.LogInformation("Registrando usuario: {Email}", registerDto.Email);
            
            var existingUser = await _userRepository.GetUserByEmail(registerDto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("El usuario con email {Email} ya existe", registerDto.Email);
                throw new InvalidOperationException("El usuario ya existe");
            }

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

            _logger.LogInformation("Usuario {Email} registrado exitosamente", registerDto.Email);
            
            return (accessToken, refreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar usuario {Email}", registerDto.Email);
            throw;
        }
    }

    public async Task<(string NewAccessToken, string NewRefreshToken)> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        try
        {
            _logger.LogInformation("Refrescando token");
            
            // Get user from expired Token
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            User user;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new SecurityTokenException("Token inválido");
            }
            
            
            if (int.TryParse(userIdClaim, out int userId))
            {
                user = await _userRepository.GetUserById(userId);
                _logger.LogInformation("Buscando usuario por ID: {UserId}", userId);
            }
            else
            {
                user = await _userRepository.GetUserByEmail(userIdClaim);
                _logger.LogInformation("Buscando usuario por email: {Email}", userIdClaim);
            }
            
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado para refresh token");
                throw new SecurityTokenException("Usuario no encontrado");
            }
            
            if (user.RefreshToken != refreshToken)
            {
                _logger.LogWarning("Refresh token no coincide para el usuario {UserId}", user.Id);
                throw new SecurityTokenException("Refresh token inválido");
            }
            
            if (user.RefreshTokenExpiryDate <= DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token expirado para el usuario {UserId}", user.Id);
                throw new SecurityTokenException("Refresh token expirado");
            }

            // Generate new Token
            var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenExpirationDays = _tokenService.GetRefreshTokenExpirationDays();
            
            // Update refresh token in db
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(refreshTokenExpirationDays);
            await _userRepository.UpdateUser(user);

            _logger.LogInformation("Tokens refrescados exitosamente para el usuario {UserId}", user.Id);
            return (newAccessToken, newRefreshToken);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogError(ex, "Error de seguridad al refrescar token: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al refrescar token: {Message}", ex.Message);
            throw new ApplicationException($"Error al refrescar token: {ex.Message}", ex);
        }
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