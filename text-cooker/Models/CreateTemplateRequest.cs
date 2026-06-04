namespace text_cooker.Models;

public class CreateTemplateRequest
{
    public string Name { get; set; } = default!;
    /// <summary>
    /// Содержимое шаблона. Может быть:
    /// 1. JSON-структура шаблона (например: {"sections":[{"type":"header","content":"Title"}]})
    /// 2. Обычный текст (будет автоматически обернут в JSON структуру)
    /// </summary>
    public string RawText { get; set; } = default!;
    public bool IsPublic { get; set; }
}