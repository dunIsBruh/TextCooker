namespace text_cooker.Models;

public class CreateDocumentRequest
{
    public string Title { get; set; } = default!;
    public string InitialText { get; set; } = default!;
}