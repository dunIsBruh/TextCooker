using System.Text.Json.Serialization;

namespace text_cooker.Models;

public class TransformRules
{
    [JsonPropertyName("normalizeWhitespace")]
    public bool NormalizeWhitespace { get; set; } = true;

    [JsonPropertyName("normalizePunctuation")]
    public bool NormalizePunctuation { get; set; } = true;

    [JsonPropertyName("capitalizeSentences")]
    public bool CapitalizeSentences { get; set; } = true;

    // Additional formatting meta (for later export)
    [JsonPropertyName("postProcessFormatting")]
    public Dictionary<string, string>? PostProcessFormatting { get; set; }
}