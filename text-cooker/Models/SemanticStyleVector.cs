using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace text_cooker.Models;

public class SemanticStyleVector
{
    // Основной семантический embedding (опционально, может быть null)
    [JsonPropertyName("embedding")]
    public float[]? Embedding { get; set; }

    // Глобальные числовые признаки (0..1, где применимо)
    [JsonPropertyName("formality")]           // 0 - casual, 1 - very formal
    public float Formality { get; set; }

    [JsonPropertyName("emotionality")]        // 0 - neutral, 1 - very emotional
    public float Emotionality { get; set; }

    [JsonPropertyName("avg_sentence_length")] // средняя длина предложения (в словах)
    public float AvgSentenceLength { get; set; }

    [JsonPropertyName("vocab_richness")]     // type-token ratio (0..1)
    public float VocabRichness { get; set; }

    [JsonPropertyName("paragraph_count")]
    public int ParagraphCount { get; set; }

    [JsonPropertyName("avg_paragraph_length")] // среднее количество слов в абзаце
    public float AvgParagraphLength { get; set; }

    // Частота ключевых слов (можно заполнять на основе шаблона)
    [JsonPropertyName("keyword_frequency")]
    public Dictionary<string, float> KeywordFrequency { get; set; } = new();

    // Доп. статистика (процент вопросов, восклицаний, доля местоимений и т.д.)
    [JsonPropertyName("question_rate")]       // % предложений, заканчивающихся вопросом
    public float QuestionRate { get; set; }

    [JsonPropertyName("exclamation_rate")]    // % предложений с восклицанием
    public float ExclamationRate { get; set; }

    [JsonPropertyName("pronoun_ratio")]       // доля местоимений среди токенов
    public float PronounRatio { get; set; }

    // Полезные метаданные
    [JsonPropertyName("computed_at")]
    public DateTime ComputedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("source_summary")]
    public string? SourceSummary { get; set; } // короткое текстовое описание стиля (опционально)

    // Serialize helper
    public string ToJson(JsonSerializerOptions? opts = null)
        => JsonSerializer.Serialize(this, opts ?? new JsonSerializerOptions { WriteIndented = true });
}
