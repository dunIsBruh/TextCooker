namespace text_cooker.Helper;

/// <summary>
/// Настройки для локального embedding сервиса
/// </summary>
public class LocalEmbeddingOptions
{
    /// <summary>
    /// Название модели (по умолчанию nomic-embed-text)
    /// </summary>
    public string ModelName { get; set; } = "nomic-embed-text";

    /// <summary>
    /// Базовый URL Ollama сервера (по умолчанию http://localhost:11434)
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:11434";

    /// <summary>
    /// Таймаут для запросов в секундах (по умолчанию 120)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Автоматически загружать модель если она не найдена
    /// </summary>
    public bool AutoPullModel { get; set; } = true;

    /// <summary>
    /// Включить подробное логирование
    /// </summary>
    public bool EnableVerboseLogging { get; set; } = false;

    /// <summary>
    /// Максимальная длина текста для embedding (символы)
    /// </summary>
    public int MaxTextLength { get; set; } = 8192;

    /// <summary>
    /// Альтернативные модели для fallback
    /// </summary>
    public List<string> FallbackModels { get; set; } = new()
    {
    };

    /// <summary>
    /// Создает настройки по умолчанию
    /// </summary>
    public static LocalEmbeddingOptions Default => new();

    /// <summary>
    /// Создает настройки для быстрой разработки (короткие таймауты, подробное логирование)
    /// </summary>
    public static LocalEmbeddingOptions Development => new()
    {
        TimeoutSeconds = 30,
        EnableVerboseLogging = true,
        AutoPullModel = false // В dev режиме не тянем модели автоматически
    };

    /// <summary>
    /// Создает настройки для продакшена (длинные таймауты, минимальное логирование)
    /// </summary>
    public static LocalEmbeddingOptions Production => new()
    {
        TimeoutSeconds = 300,
        EnableVerboseLogging = false,
        AutoPullModel = true
    };
}
