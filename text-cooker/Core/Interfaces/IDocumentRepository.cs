using text_cooker.Entities;

namespace text_cooker.Core.Interfaces;

public interface IDocumentRepository
{
    Task<int> CreateAsync(Document document);
    Task<Document?> GetAsync(int id);
    Task UpdateTimestampAsync(int id);
    Task<IEnumerable<Document>> GetAllAsync(int userId);
}