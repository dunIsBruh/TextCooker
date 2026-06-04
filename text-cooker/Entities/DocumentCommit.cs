namespace text_cooker.Entities;

public class DocumentCommit
{
    public int CommitId { get; set; }
    public int DocumentId { get; set; }
    public int ChangeStart { get; set; }
    public int ChangeEnd { get; set; }
    public int VersionNumber { get; set; }
    public int? TemplateId { get; set; }
    public string Snapshot { get; set; } = default!;  // (snapshot)
    public DateTime CreatedAt { get; set; }
}