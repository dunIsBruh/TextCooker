using System.Text.Json.Serialization;

namespace text_cooker.Models;

public class AnalyzeStyleRequest
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("keywords")]
    public List<string>? Keywords { get; set; }
}

public class CompareStylesRequest
{
    [JsonPropertyName("text1")]
    public string Text1 { get; set; } = string.Empty;

    [JsonPropertyName("text2")]
    public string Text2 { get; set; } = string.Empty;
}
