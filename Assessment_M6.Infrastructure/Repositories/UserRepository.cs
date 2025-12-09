using Assessment_M6.Domain.Entities;
using Assessment_M6.Domain.Interfaces;
using Assessment_M6.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Assessment_M6.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserById(int id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetAllUsers()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User> AddUser(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> UpdateUser(User user)
    {
        var existing = await _context.Users.FindAsync(user.Id);

        if (existing != null)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }
        return null;
    }

    public async Task<User?> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return user;  
        }
        return null;
    }
}