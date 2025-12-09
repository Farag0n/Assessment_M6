using Assessment_M6.Domain.Entities;

namespace Assessment_M6.Domain.Interfaces;

public interface IDepartmentRepository
{
    public Task<Department?> GetDepartmentById(int id);
    public Task<Department?> GetDepartmentByName(string name);
    public Task<IEnumerable<Department>> GetAllDepartments();
    
    public Task<Department> AddDepartment(Department department);
    public Task<Department?> UpdateDepartment(Department department);
    public Task<Department?> DeleteDepartment(int id);
}