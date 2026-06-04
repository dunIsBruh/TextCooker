using System.Net.Http;
using System.Text;
using System.Text.Json;
using text_cooker.Core.Interfaces;

namespace text_cooker.Engines;

/// <summary>
/// Локальный embedding сервис на базе Ollama /api/embeddings.
/// Работает с моделями типа nomic-embed-text.
/// </summary>
public class EmbeddingService(
    string modelName = "nomic-embed-text",
    string baseUrl = "http://localhost:11434")
    : IEmbeddingService, IDisposable
{
    private readonly HttpClient _http = new()
    {
        BaseAddress = new Uri(baseUrl)
    };
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    // TODO: использовать один сериализатор
    private readonly string _baseUrl = baseUrl;

    public async Task<float[]?> GetEmbeddingAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var request = new
        {
            model = modelName,
            prompt = text
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync("/api/embeddings", content, ct);

        if (!response.IsSuccessStatusCode)
            return null;

        var respJson = await response.Content.ReadAsStringAsync(ct);
        var model = JsonSerializer.Deserialize<OllamaEmbeddingResponse>(respJson, _jsonOptions);

        return model?.Embedding;
    }

    public void Dispose()
    {
        _http.Dispose();
    }

    private class OllamaEmbeddingResponse
    {
        public float[] Embedding { get; set; } = [];
    }
}