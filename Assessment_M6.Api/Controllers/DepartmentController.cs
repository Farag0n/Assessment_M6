using Assessment_M6.Application.DTOs;
using Assessment_M6.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Assessment_M6.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Solo administradores pueden acceder
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly ILogger<DepartmentController> _logger;

    public DepartmentController(
        IDepartmentService departmentService,
        ILogger<DepartmentController> logger)
    {
        _departmentService = departmentService;
        _logger = logger;
    }

    // GET: api/Departments
    [HttpGet]
    public async Task<IActionResult> GetAllDepartments()
    {
        try
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            return Ok(departments);
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Error al obtener todos los departamentos");
            return StatusCode(500, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener departamentos");
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }

    // GET: api/Departments/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDepartmentById(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { Message = "ID inválido" });
            }

            var department = await _departmentService.GetDepartmentByIdAsync(id);
            
            if (department == null)
            {
                return NotFound(new { Message = $"Departamento con ID {id} no encontrado" });
            }

            return Ok(department);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Error al obtener departamento con ID {Id}", id);
            return StatusCode(500, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener departamento con ID {Id}", id);
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }

    // GET: api/Departments/name/{name}
    [HttpGet("name/{name}")]
    public async Task<IActionResult> GetDepartmentByName(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { Message = "El nombre del departamento es requerido" });
            }

            var department = await _departmentService.GetDepartmentByNameAsync(name);
            
            if (department == null)
            {
                return NotFound(new { Message = $"Departamento '{name}' no encontrado" });
            }

            return Ok(department);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Error al buscar departamento por nombre {Name}", name);
            return StatusCode(500, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar departamento por nombre {Name}", name);
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }

    // POST: api/Departments
    [HttpPost]
    public async Task<IActionResult> CreateDepartment([FromBody] DepartmentDtos.DepartmentCreateDto departmentCreateDto)
    {
        try
        {
            if (departmentCreateDto == null)
            {
                return BadRequest(new { Message = "Los datos del departamento son requeridos" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Datos inválidos", Errors = ModelState.Values.SelectMany(v => v.Errors) });
            }

            var createdDepartment = await _departmentService.AddDepartmentAsync(departmentCreateDto);
            
            return CreatedAtAction(
                nameof(GetDepartmentById), 
                new { id = createdDepartment.Id }, 
                new { 
                    Message = "Departamento creado exitosamente", 
                    Department = createdDepartment 
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
            _logger.LogError(ex, "Error al crear departamento");
            return StatusCode(500, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear departamento");
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }

    // PUT: api/Departments/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] DepartmentDtos.DepartmentUpdateDto departmentUpdateDto)
    {
        try
        {
            if (departmentUpdateDto == null)
            {
                return BadRequest(new { Message = "Los datos de actualización son requeridos" });
            }

            if (id != departmentUpdateDto.Id)
            {
                return BadRequest(new { Message = "El ID en la URL no coincide con el ID en el cuerpo de la solicitud" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Datos inválidos", Errors = ModelState.Values.SelectMany(v => v.Errors) });
            }

            var updatedDepartment = await _departmentService.UpdateDepartmentAsync(departmentUpdateDto);
            
            if (updatedDepartment == null)
            {
                return NotFound(new { Message = $"Departamento con ID {id} no encontrado" });
            }

            return Ok(new { 
                Message = "Departamento actualizado exitosamente", 
                Department = updatedDepartment 
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
            _logger.LogError(ex, "Error al actualizar departamento con ID {Id}", id);
            return StatusCode(500, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar departamento con ID {Id}", id);
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }

    // DELETE: api/Departments/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { Message = "ID inválido" });
            }

            var deletedDepartment = await _departmentService.DeleteDepartmentAsync(id);
            
            if (deletedDepartment == null)
            {
                return NotFound(new { Message = $"Departamento con ID {id} no encontrado" });
            }

            return Ok(new { 
                Message = "Departamento eliminado exitosamente", 
                Department = deletedDepartment 
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Error al eliminar departamento con ID {Id}", id);
            return StatusCode(500, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al eliminar departamento con ID {Id}", id);
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }
}