using System.Text.Json.Serialization;

namespace text_cooker.Models;

public class TemplateStructure
{
    [JsonPropertyName("sections")]
    public List<TemplateSection> Sections { get; set; } = new();

    [JsonPropertyName("transformations")]
    public TransformRules? Transformations { get; set; } = new();

    [JsonPropertyName("style_vector")]
    public SemanticStyleVector? StyleVector { get; set; }
}