namespace text_cooker.Entities;

public class Template
{
    public int TemplateId { get; set; }
    public string Name { get; set; } = default!;
    public string Text { get; set; } = default!;
    public int OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsPublic { get; set; }
}