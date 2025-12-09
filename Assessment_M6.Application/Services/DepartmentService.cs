using Assessment_M6.Application.DTOs;
using Assessment_M6.Application.Interfaces;
using Assessment_M6.Domain.Entities;
using Assessment_M6.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Assessment_M6.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(IDepartmentRepository departmentRepository, ILogger<DepartmentService> logger)
    {
        _departmentRepository = departmentRepository;
        _logger = logger;
    }

    public async Task<DepartmentDtos.DepartmentResponseDto?> GetDepartmentByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Obteniendo departamento con ID: {Id}", id);
            
            var department = await _departmentRepository.GetDepartmentById(id);
            
            if (department == null)
            {
                _logger.LogWarning("Departamento con ID {Id} no encontrado", id);
                return null;
            }
            
            _logger.LogInformation("Departamento con ID {Id} obtenido exitosamente", id);
            return MapToDepartmentResponseDto(department);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener departamento con ID {Id}: {Message}", id, ex.Message);
            throw new ApplicationException($"Error al obtener departamento: {ex.Message}", ex);
        }
    }

    public async Task<DepartmentDtos.DepartmentResponseDto?> GetDepartmentByNameAsync(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("Nombre de departamento no puede ser nulo o vacío");
                throw new ArgumentException("El nombre del departamento es requerido");
            }
            
            _logger.LogInformation("Buscando departamento con nombre: {Name}", name);
            
            var department = await _departmentRepository.GetDepartmentByName(name);
            
            if (department == null)
            {
                _logger.LogWarning("Departamento con nombre {Name} no encontrado", name);
                return null;
            }
            
            _logger.LogInformation("Departamento con nombre {Name} encontrado exitosamente", name);
            return MapToDepartmentResponseDto(department);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar departamento por nombre {Name}: {Message}", name, ex.Message);
            throw new ApplicationException($"Error al buscar departamento: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<DepartmentDtos.DepartmentResponseDto>> GetAllDepartmentsAsync()
    {
        try
        {
            _logger.LogInformation("Obteniendo todos los departamentos");
            
            var departments = await _departmentRepository.GetAllDepartments();
            
            _logger.LogInformation("Se encontraron {Count} departamentos", departments.Count());
            return departments.Select(MapToDepartmentResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los departamentos: {Message}", ex.Message);
            throw new ApplicationException($"Error al obtener departamentos: {ex.Message}", ex);
        }
    }

    public async Task<DepartmentDtos.DepartmentResponseDto> AddDepartmentAsync(DepartmentDtos.DepartmentCreateDto departmentCreateDto)
    {
        try
        {
            if (departmentCreateDto == null)
            {
                _logger.LogWarning("Datos de departamento no pueden ser nulos");
                throw new ArgumentNullException(nameof(departmentCreateDto), "Los datos del departamento son requeridos");
            }
            
            if (string.IsNullOrWhiteSpace(departmentCreateDto.Name))
            {
                _logger.LogWarning("Nombre de departamento no puede ser nulo o vacío");
                throw new ArgumentException("El nombre del departamento es requerido");
            }
            
            _logger.LogInformation("Creando nuevo departamento: {Name}", departmentCreateDto.Name);
            
            // Verificar si el nombre del departamento ya existe
            var existingDepartment = await _departmentRepository.GetDepartmentByName(departmentCreateDto.Name);
            if (existingDepartment != null)
            {
                _logger.LogWarning("Ya existe un departamento con el nombre: {Name}", departmentCreateDto.Name);
                throw new InvalidOperationException($"Ya existe un departamento con el nombre: {departmentCreateDto.Name}");
            }
            
            var department = new Department
            {
                Name = departmentCreateDto.Name.Trim(),
                Description = departmentCreateDto.Description?.Trim() ?? string.Empty
            };

            var createdDepartment = await _departmentRepository.AddDepartment(department);
            
            _logger.LogInformation("Departamento creado exitosamente con ID: {Id}", createdDepartment.Id);
            return MapToDepartmentResponseDto(createdDepartment);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Error de validación: {Message}", ex.Message);
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Error de validación: {Message}", ex.Message);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error de operación: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear departamento: {Message}", ex.Message);
            throw new ApplicationException($"Error al crear departamento: {ex.Message}", ex);
        }
    }

    public async Task<DepartmentDtos.DepartmentResponseDto?> UpdateDepartmentAsync(DepartmentDtos.DepartmentUpdateDto departmentUpdateDto)
    {
        try
        {
            if (departmentUpdateDto == null)
            {
                _logger.LogWarning("Datos de actualización no pueden ser nulos");
                throw new ArgumentNullException(nameof(departmentUpdateDto), "Los datos de actualización son requeridos");
            }
            
            if (departmentUpdateDto.Id <= 0)
            {
                _logger.LogWarning("ID de departamento inválido: {Id}", departmentUpdateDto.Id);
                throw new ArgumentException("ID de departamento inválido");
            }
            
            if (string.IsNullOrWhiteSpace(departmentUpdateDto.Name))
            {
                _logger.LogWarning("Nombre de departamento no puede ser nulo o vacío");
                throw new ArgumentException("El nombre del departamento es requerido");
            }
            
            _logger.LogInformation("Actualizando departamento con ID: {Id}", departmentUpdateDto.Id);
            
            var existingDepartment = await _departmentRepository.GetDepartmentById(departmentUpdateDto.Id);
            if (existingDepartment == null)
            {
                _logger.LogWarning("Departamento con ID {Id} no encontrado para actualizar", departmentUpdateDto.Id);
                return null;
            }
            
            if (existingDepartment.Name != departmentUpdateDto.Name.Trim())
            {
                var departmentWithSameName = await _departmentRepository.GetDepartmentByName(departmentUpdateDto.Name.Trim());
                if (departmentWithSameName != null && departmentWithSameName.Id != departmentUpdateDto.Id)
                {
                    _logger.LogWarning("Ya existe otro departamento con el nombre: {Name}", departmentUpdateDto.Name);
                    throw new InvalidOperationException($"Ya existe otro departamento con el nombre: {departmentUpdateDto.Name}");
                }
            }

            existingDepartment.Name = departmentUpdateDto.Name.Trim();
            existingDepartment.Description = departmentUpdateDto.Description?.Trim() ?? string.Empty;

            var updatedDepartment = await _departmentRepository.UpdateDepartment(existingDepartment);
            
            if (updatedDepartment == null)
            {
                _logger.LogWarning("Error al actualizar departamento con ID: {Id}", departmentUpdateDto.Id);
                return null;
            }
            
            _logger.LogInformation("Departamento con ID {Id} actualizado exitosamente", departmentUpdateDto.Id);
            return MapToDepartmentResponseDto(updatedDepartment);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Error de validación: {Message}", ex.Message);
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Error de validación: {Message}", ex.Message);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error de operación: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar departamento: {Message}", ex.Message);
            throw new ApplicationException($"Error al actualizar departamento: {ex.Message}", ex);
        }
    }

    public async Task<DepartmentDtos.DepartmentResponseDto?> DeleteDepartmentAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                _logger.LogWarning("ID de departamento inválido: {Id}", id);
                throw new ArgumentException("ID de departamento inválido");
            }
            
            _logger.LogInformation("Eliminando departamento con ID: {Id}", id);
            
            var department = await _departmentRepository.GetDepartmentById(id);
            
            if (department == null)
            {
                _logger.LogWarning("Departamento con ID {Id} no encontrado para eliminar", id);
                return null;
            }
            
            if (department.Employees != null && department.Employees.Any())
            {
                _logger.LogWarning("No se puede eliminar el departamento {Name} porque tiene {Count} empleados asignados", 
                    department.Name, department.Employees.Count);
                throw new InvalidOperationException($"No se puede eliminar el departamento porque tiene {department.Employees.Count} empleado(s) asignado(s)");
            }

            var deletedDepartment = await _departmentRepository.DeleteDepartment(id);
            
            if (deletedDepartment == null)
            {
                _logger.LogWarning("Error al eliminar departamento con ID: {Id}", id);
                return null;
            }
            
            _logger.LogInformation("Departamento con ID {Id} eliminado exitosamente", id);
            return MapToDepartmentResponseDto(deletedDepartment);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Error de validación: {Message}", ex.Message);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error de operación: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar departamento con ID {Id}: {Message}", id, ex.Message);
            throw new ApplicationException($"Error al eliminar departamento: {ex.Message}", ex);
        }
    }
    
    private DepartmentDtos.DepartmentResponseDto MapToDepartmentResponseDto(Department department)
    {
        return new DepartmentDtos.DepartmentResponseDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description
        };
    }
}