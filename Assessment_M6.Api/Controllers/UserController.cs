using Assessment_M6.Application.DTOs;
using Assessment_M6.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Assessment_M6.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // GET: api/Users
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los usuarios");
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }
    
    [HttpGet("{id:int}")] // <-- Agrega :int para forzar parámetro numérico
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { Message = "ID inválido" });
            }

            var currentUserId = GetCurrentUserId();
            
            // Si no es admin, solo puede ver su propio perfil
            if (!User.IsInRole("Admin") && currentUserId != id)
            {
                return Forbid();
            }

            var user = await _userService.GetUserByIdAsync(id);
            
            if (user == null)
            {
                return NotFound(new { Message = $"Usuario con ID {id} no encontrado" });
            }

            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario con ID {Id}", id);
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }

    [HttpGet("email/{email}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        try
        {
            _logger.LogInformation("Buscando usuario por email: {Email}", email);
        
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { Message = "Email es requerido" });
            }

            // DEBUG: Verificar todos los usuarios
            var allUsers = await _userService.GetAllUsersAsync();
            _logger.LogInformation("Usuarios en BD: {Count}", allUsers.Count());
            foreach (var user in allUsers)
            {
                _logger.LogInformation("Usuario: ID={Id}, Email={Email}", user.Id, user.Email);
            }

            var userResponse = await _userService.GetUserByEmailAsync(email);
        
            if (userResponse == null)
            {
                _logger.LogWarning("Usuario con email '{Email}' no encontrado en GetUserByEmailAsync", email);
                return NotFound(new { Message = $"Usuario con email '{email}' no encontrado" });
            }

            _logger.LogInformation("Usuario encontrado: ID={Id}", userResponse.Id);
            return Ok(userResponse);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar usuario por email {Email}", email);
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }

    // POST: api/Users
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] UserDtos.UserCreateDTO userCreateDto)
    {
        try
        {
            if (userCreateDto == null)
            {
                return BadRequest(new { Message = "Los datos del usuario son requeridos" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { 
                    Message = "Datos inválidos", 
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) 
                });
            }

            var createdUser = await _userService.CreateUserAsync(userCreateDto);
            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario");
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }
    
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UserDtos.UserUpdateDTO userUpdateDto)
    {
        try
        {
            if (userUpdateDto == null)
            {
                return BadRequest(new { Message = "Los datos de actualización son requeridos" });
            }

            if (id != userUpdateDto.Id)
            {
                return BadRequest(new { Message = "El ID en la URL no coincide con el ID en el cuerpo" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { 
                    Message = "Datos inválidos", 
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) 
                });
            }

            // Si no es admin, solo puede actualizar su propio perfil
            var currentUserId = GetCurrentUserId();
            if (!User.IsInRole("Admin") && currentUserId != id)
            {
                return Forbid();
            }

            var updatedUser = await _userService.UpdateUserAsync(userUpdateDto);
            
            if (updatedUser == null)
            {
                return NotFound(new { Message = $"Usuario con ID {id} no encontrado" });
            }

            return Ok(updatedUser);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario con ID {Id}", id);
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }

    
    [HttpDelete("{id:int}")] // <-- Agrega :int
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { Message = "ID inválido" });
            }

            var deletedUser = await _userService.DeleteUserAsync(id);
            
            if (deletedUser == null)
            {
                return NotFound(new { Message = $"Usuario con ID {id} no encontrado" });
            }

            return Ok(new { 
                Message = "Usuario eliminado exitosamente", 
                User = deletedUser 
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario con ID {Id}", id);
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }

    // Método auxiliar para obtener el ID del usuario actual
    private int GetCurrentUserId()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("No se encontró el claim NameIdentifier en el token");
                return 0;
            }
            
            // Intenta parsear como entero
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            
            _logger.LogWarning("El claim NameIdentifier no es un ID numérico: {Value}", userIdClaim);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ID del usuario actual");
            return 0;
        }
    }
    
    
}