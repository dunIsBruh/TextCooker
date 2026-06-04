namespace text_cooker.Entities;

public class Document
{
    public int DocumentId { get; set; }
    public string Title { get; set; } = default!;
    public int OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}