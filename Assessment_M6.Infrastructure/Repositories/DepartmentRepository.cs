using Assessment_M6.Domain.Entities;
using Assessment_M6.Domain.Interfaces;
using Assessment_M6.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Assessment_M6.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _context;

    public DepartmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Department?> GetDepartmentById(int id)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Department?> GetDepartmentByName(string name)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(d => d.Name == name);
    }

    public async Task<IEnumerable<Department>> GetAllDepartments()
    {
        return await _context.Departments.ToListAsync();
    }

    public async Task<Department> AddDepartment(Department department)
    {
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task<Department?> UpdateDepartment(Department department)
    {
        var existing = await _context.Departments.FindAsync(department.Id);

        if (existing != null)
        {
            _context.Departments.Update(department);
            await _context.SaveChangesAsync();
            return department;
        }
        return null;
    }

    public async Task<Department?> DeleteDepartment(int id)
    {
        var department = await _context.Departments.FindAsync(id);

        if (department != null)
        {
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            return department;  
        }
        return null;
    }
}