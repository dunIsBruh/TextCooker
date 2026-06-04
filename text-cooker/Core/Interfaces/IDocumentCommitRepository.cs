using text_cooker.Entities;

namespace text_cooker.Core.Interfaces;

public interface IDocumentCommitRepository
{
    Task<int> CreateAsync(DocumentCommit commit);
    Task<DocumentCommit?> GetLastAsync(int documentId);
    Task<List<DocumentCommit>> GetHistoryAsync(int documentId);
}