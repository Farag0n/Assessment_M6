using Assessment_M6.Application.DTOs;
using Assessment_M6.Application.Interfaces;
using Assessment_M6.Domain.Entities;
using Assessment_M6.Domain.Enums;
using Assessment_M6.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Assessment_M6.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IEmployeeRepository employeeRepository, 
        IDepartmentRepository departmentRepository,
        ILogger<EmployeeService> logger)
    {
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
        _logger = logger;
    }

    public async Task<EmployeeDtos.EmployeeResponseDto?> GetEmployeeByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                _logger.LogWarning("ID de empleado inválido: {Id}", id);
                throw new ArgumentException("ID de empleado inválido");
            }
            
            _logger.LogInformation("Obteniendo empleado con ID: {Id}", id);
            
            var employee = await _employeeRepository.GetEmployeeById(id);
            
            if (employee == null)
            {
                _logger.LogWarning("Empleado con ID {Id} no encontrado", id);
                return null;
            }
            
            await LoadDepartmentData(employee);
            
            _logger.LogInformation("Empleado con ID {Id} obtenido exitosamente", id);
            return MapToEmployeeResponseDto(employee);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Error de validación: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empleado con ID {Id}: {Message}", id, ex.Message);
            throw new ApplicationException($"Error al obtener empleado: {ex.Message}", ex);
        }
    }

    public async Task<EmployeeDtos.EmployeeResponseDto?> GetEmployeeByEmailAsync(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Email no puede ser nulo o vacío");
                throw new ArgumentException("El email es requerido");
            }
            
            if (!IsValidEmail(email))
            {
                _logger.LogWarning("Email con formato inválido: {Email}", email);
                throw new ArgumentException("El email no tiene un formato válido");
            }
            
            _logger.LogInformation("Buscando empleado con email: {Email}", email);
            
            var employee = await _employeeRepository.GetEmployeeByEmail(email);
            
            if (employee == null)
            {
                _logger.LogWarning("Empleado con email {Email} no encontrado", email);
                return null;
            }
            
            await LoadDepartmentData(employee);
            
            _logger.LogInformation("Empleado con email {Email} encontrado exitosamente", email);
            return MapToEmployeeResponseDto(employee);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Error de validación: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar empleado por email {Email}: {Message}", email, ex.Message);
            throw new ApplicationException($"Error al buscar empleado: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<EmployeeDtos.EmployeeResponseDto>> GetAllEmployeesAsync()
    {
        try
        {
            _logger.LogInformation("Obteniendo todos los empleados");
            
            var employees = await _employeeRepository.GetAllEmployees();
            var employeeList = employees.ToList();
            
            _logger.LogInformation("Se encontraron {Count} empleados", employeeList.Count);
            
            foreach (var employee in employeeList)
            {
                await LoadDepartmentData(employee);
            }
            
            return employeeList.Select(MapToEmployeeResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los empleados: {Message}", ex.Message);
            throw new ApplicationException($"Error al obtener empleados: {ex.Message}", ex);
        }
    }

    public async Task<EmployeeDtos.EmployeeResponseDto> AddEmployeeAsync(EmployeeDtos.EmployeeCreateDto employeeCreateDto)
    {
        try
        {
            if (employeeCreateDto == null)
            {
                _logger.LogWarning("Datos de empleado no pueden ser nulos");
                throw new ArgumentNullException(nameof(employeeCreateDto), "Los datos del empleado son requeridos");
            }
            
            ValidateEmployeeData(employeeCreateDto);
            
            _logger.LogInformation("Creando nuevo empleado: {Name} {LastName}", 
                employeeCreateDto.Name, employeeCreateDto.LastName);
            
            var existingEmployee = await _employeeRepository.GetEmployeeByEmail(employeeCreateDto.Email);
            if (existingEmployee != null)
            {
                _logger.LogWarning("El email {Email} ya está registrado para otro empleado", employeeCreateDto.Email);
                throw new InvalidOperationException($"El email {employeeCreateDto.Email} ya está registrado para otro empleado");
            }
            
            var allEmployees = await _employeeRepository.GetAllEmployees();
            var existingDocNumber = allEmployees.FirstOrDefault(e => e.DocNumber == employeeCreateDto.DocNumber);
            if (existingDocNumber != null)
            {
                _logger.LogWarning("El número de documento {DocNumber} ya está registrado", employeeCreateDto.DocNumber);
                throw new InvalidOperationException($"El número de documento {employeeCreateDto.DocNumber} ya está registrado");
            }
            
            var department = await _departmentRepository.GetDepartmentById(employeeCreateDto.DepartmentId);
            if (department == null)
            {
                _logger.LogWarning("Departamento con ID {DepartmentId} no encontrado", employeeCreateDto.DepartmentId);
                throw new InvalidOperationException($"El departamento con ID {employeeCreateDto.DepartmentId} no existe");
            }

            var employee = new Employee
            {
                Name = employeeCreateDto.Name.Trim(),
                LastName = employeeCreateDto.LastName.Trim(),
                Age = employeeCreateDto.Age,
                DocNumber = employeeCreateDto.DocNumber.Trim(),
                Email = employeeCreateDto.Email.Trim(),
                PhoneNumber = employeeCreateDto.PhoneNumber.Trim(),
                State = employeeCreateDto.State,
                DepartmentId = employeeCreateDto.DepartmentId,
                RegisteredAt = DateTime.UtcNow
            };

            var createdEmployee = await _employeeRepository.AddEmployee(employee);
            
            // Add department info
            createdEmployee.Department = department;
            
            _logger.LogInformation("Empleado creado exitosamente con ID: {Id}", createdEmployee.Id);
            return MapToEmployeeResponseDto(createdEmployee);
        }
        //se usaron varios catch para ser mas especificos
        catch (ArgumentNullException ex)//si el parametro es null
        {
            _logger.LogError(ex, "Error de validación: {Message}", ex.Message);
            throw;
        }
        catch (ArgumentException ex)//argumento invalido
        {
            _logger.LogError(ex, "Error de validación: {Message}", ex.Message);
            throw;
        }
        catch (InvalidOperationException ex)//operacion no es valida
        {
            _logger.LogError(ex, "Error de operación: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)//catch general, el original
        {
            _logger.LogError(ex, "Error al crear empleado: {Message}", ex.Message);
            throw new ApplicationException($"Error al crear empleado: {ex.Message}", ex);
        }
    }

    public async Task<EmployeeDtos.EmployeeResponseDto?> UpdateEmployeeAsync(EmployeeDtos.EmployeeUpdateDto employeeUpdateDto)
    {
        try
        {
            if (employeeUpdateDto == null)
            {
                _logger.LogWarning("Datos de actualización no pueden ser nulos");
                throw new ArgumentNullException(nameof(employeeUpdateDto), "Los datos de actualización son requeridos");
            }
            
            if (employeeUpdateDto.Id <= 0)
            {
                _logger.LogWarning("ID de empleado inválido: {Id}", employeeUpdateDto.Id);
                throw new ArgumentException("ID de empleado inválido");
            }
            
            ValidateEmployeeData(employeeUpdateDto);
            
            _logger.LogInformation("Actualizando empleado con ID: {Id}", employeeUpdateDto.Id);
            
            var existingEmployee = await _employeeRepository.GetEmployeeById(employeeUpdateDto.Id);
            if (existingEmployee == null)
            {
                _logger.LogWarning("Empleado con ID {Id} no encontrado para actualizar", employeeUpdateDto.Id);
                return null;
            }
            
            if (existingEmployee.Email != employeeUpdateDto.Email.Trim())
            {
                var employeeWithSameEmail = await _employeeRepository.GetEmployeeByEmail(employeeUpdateDto.Email.Trim());
                if (employeeWithSameEmail != null && employeeWithSameEmail.Id != employeeUpdateDto.Id)
                {
                    _logger.LogWarning("El email {Email} ya está registrado por otro empleado", employeeUpdateDto.Email);
                    throw new InvalidOperationException($"El email {employeeUpdateDto.Email} ya está registrado por otro empleado");
                }
            }
            
            if (existingEmployee.DocNumber != employeeUpdateDto.DocNumber.Trim())
            {
                var allEmployees = await _employeeRepository.GetAllEmployees();
                var employeeWithSameDoc = allEmployees.FirstOrDefault(e => 
                    e.DocNumber == employeeUpdateDto.DocNumber.Trim() && e.Id != employeeUpdateDto.Id);
                if (employeeWithSameDoc != null)
                {
                    _logger.LogWarning("El número de documento {DocNumber} ya está en uso", employeeUpdateDto.DocNumber);
                    throw new InvalidOperationException($"El número de documento {employeeUpdateDto.DocNumber} ya está en uso");
                }
            }
            
            if (existingEmployee.DepartmentId != employeeUpdateDto.DepartmentId)
            {
                var newDepartment = await _departmentRepository.GetDepartmentById(employeeUpdateDto.DepartmentId);
                if (newDepartment == null)
                {
                    _logger.LogWarning("Departamento con ID {DepartmentId} no encontrado", employeeUpdateDto.DepartmentId);
                    throw new InvalidOperationException($"El departamento con ID {employeeUpdateDto.DepartmentId} no existe");
                }
                
                existingEmployee.DepartmentId = employeeUpdateDto.DepartmentId;
                existingEmployee.Department = newDepartment;
            }

            existingEmployee.Name = employeeUpdateDto.Name.Trim();
            existingEmployee.LastName = employeeUpdateDto.LastName.Trim();
            existingEmployee.Age = employeeUpdateDto.Age;
            existingEmployee.DocNumber = employeeUpdateDto.DocNumber.Trim();
            existingEmployee.Email = employeeUpdateDto.Email.Trim();
            existingEmployee.PhoneNumber = employeeUpdateDto.PhoneNumber.Trim();
            existingEmployee.State = employeeUpdateDto.State;

            var updatedEmployee = await _employeeRepository.UpdateEmployee(existingEmployee.Id, existingEmployee);
            
            if (updatedEmployee == null)
            {
                _logger.LogWarning("Error al actualizar empleado con ID: {Id}", employeeUpdateDto.Id);
                return null;
            }
            
            //Make sure the department is loaded
            await LoadDepartmentData(updatedEmployee);
            
            _logger.LogInformation("Empleado con ID {Id} actualizado exitosamente", employeeUpdateDto.Id);
            return MapToEmployeeResponseDto(updatedEmployee);
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
            _logger.LogError(ex, "Error al actualizar empleado: {Message}", ex.Message);
            throw new ApplicationException($"Error al actualizar empleado: {ex.Message}", ex);
        }
    }

    public async Task<EmployeeDtos.EmployeeResponseDto?> DeleteEmployeeAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                _logger.LogWarning("ID de empleado inválido: {Id}", id);
                throw new ArgumentException("ID de empleado inválido");
            }
            
            _logger.LogInformation("Eliminando empleado con ID: {Id}", id);
            
            var deletedEmployee = await _employeeRepository.DeleteEmployee(id);
            
            if (deletedEmployee == null)
            {
                _logger.LogWarning("Empleado con ID {Id} no encontrado para eliminar", id);
                return null;
            }
            
            await LoadDepartmentData(deletedEmployee);
            
            _logger.LogInformation("Empleado con ID {Id} eliminado exitosamente", id);
            return MapToEmployeeResponseDto(deletedEmployee);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Error de validación: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar empleado con ID {Id}: {Message}", id, ex.Message);
            throw new ApplicationException($"Error al eliminar empleado: {ex.Message}", ex);
        }
    }
    
    private async Task LoadDepartmentData(Employee employee)
    {
        try
        {
            if (employee.Department == null && employee.DepartmentId > 0)
            {
                employee.Department = await _departmentRepository.GetDepartmentById(employee.DepartmentId);
                
                if (employee.Department == null)
                {
                    _logger.LogWarning("Departamento con ID {DepartmentId} no encontrado para el empleado {EmployeeId}", 
                        employee.DepartmentId, employee.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar datos del departamento para empleado {EmployeeId}: {Message}", 
                employee.Id, ex.Message);
        }
    }
    
    private void ValidateEmployeeData(object employeeDto)
    {
        if (employeeDto == null) return;

        var properties = employeeDto.GetType().GetProperties();
        
        foreach (var property in properties)
        {
            var value = property.GetValue(employeeDto);
            
            if (property.Name == "Name" || property.Name == "LastName")
            {
                if (string.IsNullOrWhiteSpace(value as string))
                    throw new ArgumentException($"{property.Name} es requerido");
                    
                if ((value as string).Length > 100)
                    throw new ArgumentException($"{property.Name} no puede exceder 100 caracteres");
            }
            else if (property.Name == "Email")
            {
                if (string.IsNullOrWhiteSpace(value as string))
                    throw new ArgumentException("Email es requerido");
                    
                if (!IsValidEmail(value as string))
                    throw new ArgumentException("Email no tiene un formato válido");
            }
            else if (property.Name == "Age")
            {
                if ((int)value < 18 || (int)value > 100)
                    throw new ArgumentException("La edad debe estar entre 18 y 100 años");
            }
            else if (property.Name == "DocNumber")
            {
                if (string.IsNullOrWhiteSpace(value as string))
                    throw new ArgumentException("Número de documento es requerido");
            }
            else if (property.Name == "DepartmentId")
            {
                if ((int)value <= 0)
                    throw new ArgumentException("ID de departamento inválido");
            }
            else if (property.Name == "State")
            {
                if (!Enum.IsDefined(typeof(EmployeeState), value))
                    throw new ArgumentException("Estado de empleado inválido");
            }
        }
    }
    
    private bool IsValidEmail(string email)
    {
        try
        {
            //consejo de la ia para validar el formato del email
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    private EmployeeDtos.EmployeeResponseDto MapToEmployeeResponseDto(Employee employee)
    {
        var dto = new EmployeeDtos.EmployeeResponseDto
        {
            Id = employee.Id,
            Name = employee.Name,
            LastName = employee.LastName,
            Age = employee.Age,
            DocNumber = employee.DocNumber,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            State = employee.State,
            RegisteredAt = employee.RegisteredAt
        };

        //Add department info
        if (employee.Department != null)
        {
            dto.Department = new DepartmentDtos.DepartmentResponseDto
            {
                Id = employee.Department.Id,
                Name = employee.Department.Name,
                Description = employee.Department.Description
            };
        }
        else if (employee.DepartmentId > 0)
        {
            dto.Department = new DepartmentDtos.DepartmentResponseDto
            {
                Id = employee.DepartmentId,
                Name = "Departamento no disponible",
                Description = "La información del departamento no se pudo cargar"
            };
        }

        return dto;
    }
}