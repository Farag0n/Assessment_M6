using Assessment_M6.Application.DTOs;

namespace Assessment_M6.Application.Interfaces;

public interface IDepartmentService
{
    Task<DepartmentDtos.DepartmentResponseDto?> GetDepartmentByIdAsync(int id);
    Task<DepartmentDtos.DepartmentResponseDto?> GetDepartmentByNameAsync(string name);
    Task<IEnumerable<DepartmentDtos.DepartmentResponseDto>> GetAllDepartmentsAsync();
    Task<DepartmentDtos.DepartmentResponseDto> AddDepartmentAsync(DepartmentDtos.DepartmentCreateDto departmentCreateDto);
    Task<DepartmentDtos.DepartmentResponseDto?> UpdateDepartmentAsync(DepartmentDtos.DepartmentUpdateDto departmentUpdateDto);
    Task<DepartmentDtos.DepartmentResponseDto?> DeleteDepartmentAsync(int id);
    
}