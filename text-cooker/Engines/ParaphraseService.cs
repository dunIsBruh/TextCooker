using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using text_cooker.Core.Interfaces;

namespace text_cooker.Engines;

/// <summary>
/// Локальный сервис генерации текста через Ollama /api/chat.
/// Используется для переписывания текста под стиль шаблона.
/// </summary>
public class ParaphraseService(string model, string baseUrl) : IParaphraseService, IDisposable
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri(baseUrl),
        Timeout = TimeSpan.FromSeconds(200)
    };
    
    public async Task<string?> ParaphraseToStyleAsync(
        string prompt,
        CancellationToken ct = default)
    {
        var request = new
        {
            model = model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            stream = false,
            option = new {
                num_predict = 250,   // ограничиваем длину ответа
                temperature = 0.4    // более стабильный стиль
            }
        };

        var response = await _httpClient.PostAsJsonAsync("/api/chat", request, ct);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync(ct);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.TryGetProperty("message", out var msg) &&
            msg.TryGetProperty("content", out var content))
        {
            var rawContent = content.GetString();
            if (string.IsNullOrWhiteSpace(rawContent))
                return null;

            // Извлекаем полезную нагрузку, если ответ содержит разделители ---
            return ExtractPayload(rawContent);
        }
        return null;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

        /// <summary>
    /// Извлекает полезную нагрузку из ответа модели.
    /// Если ответ содержит разделители ---, возвращает текст между ними.
    /// Иначе возвращает весь текст, очищенный от лишних пробелов.
    /// </summary>
    private static string ExtractPayload(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return response;

        // Ищем паттерн: текст --- полезная нагрузка --- текст
        var parts = response.Split(new[] { "---" }, StringSplitOptions.None);
        
        // Если есть разделители ---, берем среднюю часть (полезную нагрузку)
        if (parts.Length >= 3)
        {
            // Берем среднюю часть (индекс 1), так как обычно формат:
            // parts[0] - предисловие
            // parts[1] - полезная нагрузка
            // parts[2+] - послесловие
            var payload = parts[1].Trim();
            
            // Если средняя часть пустая, пробуем взять первую непустую часть
            if (string.IsNullOrWhiteSpace(payload) && parts.Length > 1)
            {
                payload = parts.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Trim()))?.Trim() ?? response;
            }
            
            return payload;
        }

        // Если разделителей нет, возвращаем весь текст, очищенный от лишних пробелов
        return response.Trim();
    }
}
