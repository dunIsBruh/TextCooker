namespace text_cooker.Core.Interfaces;

public interface IParaphraseService
{
    Task<string?> ParaphraseToStyleAsync(string prompt, CancellationToken ct = default);
}