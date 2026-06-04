using text_cooker.Entities;

namespace text_cooker.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByLoginAsync(string login);
    Task<User?> GetByIdAsync(int id);
    Task AddAsync(User user);
    Task UpdateLastLoginAsync(int id, DateTime lastLogin);
}