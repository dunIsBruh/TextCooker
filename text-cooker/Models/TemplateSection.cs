using System.Text.Json.Serialization;

namespace text_cooker.Models;

public class TemplateSection
{
    public string Name { get; set; } = string.Empty;

    // "required", "optional", "repeatable"
    public string Type { get; set; } = "optional";

    // example paragraph for this section (inferred)
    public string? Example { get; set; }

    // keywords that hint at this section
    public List<string> Keywords { get; set; } = new();

    // optional simple regex patterns (string) to match this section
    public List<string> Patterns { get; set; } = new();
}