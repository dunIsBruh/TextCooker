using text_cooker.Entities;

namespace text_cooker.Core.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);
    Task<RefreshToken?> GetByHashAsync(string tokenHash);
    Task<IEnumerable<RefreshToken>> GetAllByUserAsync(int userId);
    Task UpdateAsync(RefreshToken token);
    Task DeleteExpiredAsync();
}