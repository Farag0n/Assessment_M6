using Assessment_M6.Application.DTOs;
using Assessment_M6.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assessment_M6.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin")] // Solo Admin puede ver todos los usuarios
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        
        if (user == null)
            return NotFound(new { Message = "Usuario no encontrado" });

        //if user don't is admin only view your id
        var currentUserId = GetCurrentUserId();
        if (!User.IsInRole("Admin") && currentUserId != id)
            return Forbid();

        return Ok(user);
    }
    
    [HttpGet("email/{email}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var user = await _userService.GetUserByEmailAsync(email);
        
        if (user == null)
            return NotFound(new { Message = "Usuario no encontrado" });

        return Ok(user);
    }

    
    [HttpPost]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] UserDtos.UserCreateDTO userCreateDto)
    {
        try
        {
            var createdUser = await _userService.CreateUserAsync(userCreateDto);
            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
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

    // PUT: api/Users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UserDtos.UserUpdateDTO userUpdateDto)
    {
        if (id != userUpdateDto.Id)
            return BadRequest(new { Message = "ID no coincide" });

        //if dont is admin only update your profile
        var currentUserId = GetCurrentUserId();
        if (!User.IsInRole("Admin") && currentUserId != id)
            return Forbid();

        try
        {
            var updatedUser = await _userService.UpdateUserAsync(userUpdateDto);
            
            if (updatedUser == null)
                return NotFound(new { Message = "Usuario no encontrado" });

            return Ok(updatedUser);
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
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Solo Admin puede eliminar usuarios
    public async Task<IActionResult> Delete(int id)
    {
        var deletedUser = await _userService.DeleteUserAsync(id);
        
        if (deletedUser == null)
            return NotFound(new { Message = "Usuario no encontrado" });

        return Ok(new { Message = "Usuario eliminado exitosamente", User = deletedUser });
    }
    
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }
}