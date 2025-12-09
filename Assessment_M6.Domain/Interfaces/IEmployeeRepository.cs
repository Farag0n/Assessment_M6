using Assessment_M6.Domain.Entities;

namespace Assessment_M6.Domain.Interfaces;

public interface IEmployeeRepository
{
    public Task<Employee?> GetEmployeeById(int id);
    public Task<Employee?> GetEmployeeByEmail(string email);
    public Task<IEnumerable<Employee>> GetAllEmployees();
    
    public Task<Employee> AddEmployee(Employee employee);
    public Task<Employee?> UpdateEmployee(int id, Employee employee);
    public Task<Employee?> DeleteEmployee(int id);
}