using Assessment_M6.Application.DTOs;

namespace Assessment_M6.Application.Interfaces;

public interface IEmployeeService
{
    Task<EmployeeDtos.EmployeeResponseDto?> GetEmployeeByIdAsync(int id);
    Task<EmployeeDtos.EmployeeResponseDto?> GetEmployeeByEmailAsync(string email);
    Task<IEnumerable<EmployeeDtos.EmployeeResponseDto>> GetAllEmployeesAsync();
    Task<EmployeeDtos.EmployeeResponseDto> AddEmployeeAsync(EmployeeDtos.EmployeeCreateDto employeeCreateDto);
    Task<EmployeeDtos.EmployeeResponseDto?> UpdateEmployeeAsync(EmployeeDtos.EmployeeUpdateDto employeeUpdateDto);
    Task<EmployeeDtos.EmployeeResponseDto?> DeleteEmployeeAsync(int id);
}