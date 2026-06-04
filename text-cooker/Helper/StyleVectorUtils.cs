using System;
using text_cooker.Models;

namespace text_cooker.Helper;

public static class StyleVectorUtils
{
    /// <summary>
    /// Вычисляет общий скор соответствия между двумя векторами стиля
    /// </summary>
    /// <param name="a">Первый вектор стиля</param>
    /// <param name="b">Второй вектор стиля</param>
    /// <returns>Скор от 0 до 1, где 1 - полное соответствие</returns>
    public static float ScoreStyleMatch(SemanticStyleVector a, SemanticStyleVector b)
    {
        if (a == null || b == null) return 0f;

        float embScore = 0f;
        if (a.Embedding != null && b.Embedding != null && a.Embedding.Length == b.Embedding.Length)
            embScore = VectorUtils.CosineFloatArray(a.Embedding, b.Embedding);

        float formalDiff = 1f - Math.Abs(a.Formality - b.Formality); // 0..1
        float emoDiff = 1f - Math.Abs(a.Emotionality - b.Emotionality);
        float lenDiff = 1f - (Math.Abs(a.AvgSentenceLength - b.AvgSentenceLength) / Math.Max(1f, (a.AvgSentenceLength + b.AvgSentenceLength)/2f));
        float vocabDiff = 1f - Math.Abs(a.VocabRichness - b.VocabRichness);

        // weights: embeddings most important if available
        float combined = 0f;
        if (a.Embedding != null && b.Embedding != null)
            combined = 0.55f*embScore + 0.15f*formalDiff + 0.15f*emoDiff + 0.10f*lenDiff + 0.05f*vocabDiff;
        else
            combined = 0.35f*formalDiff + 0.35f*emoDiff + 0.20f*lenDiff + 0.10f*vocabDiff;

        return Math.Clamp(combined, 0f, 1f);
    }

    /// <summary>
    /// Вычисляет скор соответствия по формальности
    /// </summary>
    public static float ScoreFormalityMatch(SemanticStyleVector a, SemanticStyleVector b)
    {
        if (a == null || b == null) return 0f;
        return 1f - Math.Abs(a.Formality - b.Formality);
    }

    /// <summary>
    /// Вычисляет скор соответствия по эмоциональности
    /// </summary>
    public static float ScoreEmotionalityMatch(SemanticStyleVector a, SemanticStyleVector b)
    {
        if (a == null || b == null) return 0f;
        return 1f - Math.Abs(a.Emotionality - b.Emotionality);
    }

    /// <summary>
    /// Вычисляет скор соответствия по структурным характеристикам
    /// </summary>
    public static float ScoreStructuralMatch(SemanticStyleVector a, SemanticStyleVector b)
    {
        if (a == null || b == null) return 0f;

        float lenDiff = 1f - (Math.Abs(a.AvgSentenceLength - b.AvgSentenceLength) / Math.Max(1f, (a.AvgSentenceLength + b.AvgSentenceLength)/2f));
        float vocabDiff = 1f - Math.Abs(a.VocabRichness - b.VocabRichness);
        float paraDiff = 1f - (Math.Abs(a.AvgParagraphLength - b.AvgParagraphLength) / Math.Max(1f, (a.AvgParagraphLength + b.AvgParagraphLength)/2f));

        return (lenDiff + vocabDiff + paraDiff) / 3f;
    }

    /// <summary>
    /// Вычисляет скор соответствия по ключевым словам
    /// </summary>
    public static float ScoreKeywordMatch(SemanticStyleVector a, SemanticStyleVector b)
    {
        if (a?.KeywordFrequency == null || b?.KeywordFrequency == null) return 0f;

        var allKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in a.KeywordFrequency.Keys) allKeys.Add(key);
        foreach (var key in b.KeywordFrequency.Keys) allKeys.Add(key);

        if (allKeys.Count == 0) return 1f; // если нет ключевых слов, считаем полным соответствием

        float totalDiff = 0f;
        foreach (var key in allKeys)
        {
            float freqA = a.KeywordFrequency.GetValueOrDefault(key, 0f);
            float freqB = b.KeywordFrequency.GetValueOrDefault(key, 0f);
            totalDiff += Math.Abs(freqA - freqB);
        }

        float avgDiff = totalDiff / allKeys.Count;
        return Math.Clamp(1f - avgDiff, 0f, 1f);
    }

    /// <summary>
    /// Создает краткое описание стиля на основе вектора
    /// </summary>
    public static string DescribeStyle(SemanticStyleVector vector)
    {
        if (vector == null) return "Неопределенный стиль";

        var parts = new List<string>();

        // Формальность
        if (vector.Formality > 0.7f) parts.Add("формальный");
        else if (vector.Formality < 0.3f) parts.Add("неформальный");
        else parts.Add("нейтральный");

        // Эмоциональность
        if (vector.Emotionality > 0.6f) parts.Add("эмоциональный");
        else if (vector.Emotionality < 0.2f) parts.Add("сдержанный");

        // Длина предложений
        if (vector.AvgSentenceLength > 20f) parts.Add("развернутый");
        else if (vector.AvgSentenceLength < 8f) parts.Add("лаконичный");

        // Богатство словаря
        if (vector.VocabRichness > 0.7f) parts.Add("разнообразный словарь");
        else if (vector.VocabRichness < 0.3f) parts.Add("простой словарь");

        return string.Join(", ", parts);
    }
}
