using text_cooker.Entities;

namespace text_cooker.Core.Interfaces;

public interface ITemplateRepository
{
    Task<int> CreateAsync(Template template);
    Task<Template?> GetAsync(int id);
    Task<IEnumerable<Template>> GetAvailableAsync(int ownerId);
}