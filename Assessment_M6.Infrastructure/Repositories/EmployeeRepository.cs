using Assessment_M6.Domain.Entities;
using Assessment_M6.Domain.Interfaces;
using Assessment_M6.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Assessment_M6.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetEmployeeById(int id)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Employee?> GetEmployeeByEmail(string email)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.Email == email);
    }

    public async Task<IEnumerable<Employee>> GetAllEmployees()
    {
        return await _context.Employees.ToListAsync();
    }

    public async Task<Employee> AddEmployee(Employee employee)
    {
        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee?> UpdateEmployee(int id, Employee employee)
    {
        var existing = await _context.Employees.FindAsync(id);

        if (existing != null)
        {
            existing.Name = employee.Name;
            existing.LastName = employee.LastName;
            existing.Age = employee.Age;
            existing.DocNumber = employee.DocNumber;
            existing.Email = employee.Email;
            existing.PhoneNumber = employee.PhoneNumber;
            existing.State = employee.State;
            existing.DepartmentId = employee.DepartmentId;
        
            await _context.SaveChangesAsync();
            return existing;
        }
        return null;
    }
    public async Task<Employee?> DeleteEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);

        if (employee != null)
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return employee;  
        }
        return null;
    }
}