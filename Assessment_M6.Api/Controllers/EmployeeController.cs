using Assessment_M6.Application.DTOs;
using Assessment_M6.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Assessment_M6.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(
        IEmployeeService employeeService,
        ILogger<EmployeeController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }
    
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }
    
    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllEmployees()
    {
        try
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Error al obtener todos los empleados");
            return StatusCode(500, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener empleados");
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }

    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployeeById(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { Message = "ID inválido" });
            }
            
            var currentUserId = GetCurrentUserId();
            if (!IsAdmin() && id != currentUserId)
            {
                return Forbid(); // 403 Forbidden
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            
            if (employee == null)
            {
                return NotFound(new { Message = $"Empleado con ID {id} no encontrado" });
            }

            return Ok(employee);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Error al obtener empleado con ID {Id}", id);
            return StatusCode(500, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener empleado con ID {Id}", id);
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }
    
    [HttpGet("email/{email}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetEmployeeByEmail(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { Message = "El email es requerido" });
            }

            var employee = await _employeeService.GetEmployeeByEmailAsync(email);
            
            if (employee == null)
            {
                return NotFound(new { Message = $"Empleado con email {email} no encontrado" });
            }

            return Ok(employee);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Error al buscar empleado por email {Email}", email);
            return StatusCode(500, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar empleado por email {Email}", email);
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateEmployee([FromBody] EmployeeDtos.EmployeeCreateDto employeeCreateDto)
    {
        try
        {
            if (employeeCreateDto == null)
            {
                return BadRequest(new { Message = "Los datos del empleado son requeridos" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { 
                    Message = "Datos inválidos", 
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) 
                });
            }

            var createdEmployee = await _employeeService.AddEmployeeAsync(employeeCreateDto);
            
            return CreatedAtAction(
                nameof(GetEmployeeById), 
                new { id = createdEmployee.Id }, 
                new { 
                    Message = "Empleado creado exitosamente", 
                    Employee = createdEmployee 
                });
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Error al crear empleado");
            return StatusCode(500, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear empleado");
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeDtos.EmployeeUpdateDto employeeUpdateDto)
    {
        try
        {
            if (employeeUpdateDto == null)
            {
                return BadRequest(new { Message = "Los datos de actualización son requeridos" });
            }

            if (id != employeeUpdateDto.Id)
            {
                return BadRequest(new { Message = "El ID en la URL no coincide con el ID en el cuerpo de la solicitud" });
            }
            
            var currentUserId = GetCurrentUserId();
            if (!IsAdmin() && id != currentUserId)
            {
                return Forbid(); // 403 Forbidden
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { 
                    Message = "Datos inválidos", 
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) 
                });
            }

            var updatedEmployee = await _employeeService.UpdateEmployeeAsync(employeeUpdateDto);
            
            if (updatedEmployee == null)
            {
                return NotFound(new { Message = $"Empleado con ID {id} no encontrado" });
            }

            return Ok(new { 
                Message = "Empleado actualizado exitosamente", 
                Employee = updatedEmployee 
            });
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Error al actualizar empleado con ID {Id}", id);
            return StatusCode(500, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar empleado con ID {Id}", id);
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }

    // DELETE: api/Employees/5 (Solo Admin)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { Message = "ID inválido" });
            }

            var deletedEmployee = await _employeeService.DeleteEmployeeAsync(id);
            
            if (deletedEmployee == null)
            {
                return NotFound(new { Message = $"Empleado con ID {id} no encontrado" });
            }

            return Ok(new { 
                Message = "Empleado eliminado exitosamente", 
                Employee = deletedEmployee 
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Error al eliminar empleado con ID {Id}", id);
            return StatusCode(500, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al eliminar empleado con ID {Id}", id);
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            if (currentUserId <= 0)
            {
                return Unauthorized(new { Message = "Usuario no autenticado" });
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(currentUserId);
            
            if (employee == null)
            {
                return NotFound(new { Message = "Perfil de empleado no encontrado" });
            }

            return Ok(employee);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Error al obtener perfil del usuario actual");
            return StatusCode(500, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener perfil del usuario actual");
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }
}