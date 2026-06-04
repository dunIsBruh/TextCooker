namespace text_cooker.Core.Interfaces;

public interface IEmbeddingService
{
    // returns embedding (float vector) or null if not possible
    Task<float[]?> GetEmbeddingAsync(string text, CancellationToken ct = default);
}