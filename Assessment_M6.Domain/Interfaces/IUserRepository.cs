using Assessment_M6.Domain.Entities;

namespace Assessment_M6.Domain.Interfaces;

public interface IUserRepository
{
    public Task<User?> GetUserById(int id);
    public Task<User?> GetUserByEmail(string email);
    public Task<IEnumerable<User>> GetAllUsers();
    
    public Task<User> AddUser(User user);
    public Task<User?> UpdateUser(User user);
    public Task<User?> DeleteUser(int id);
}