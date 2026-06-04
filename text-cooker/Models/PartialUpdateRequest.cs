namespace text_cooker.Models;

public class PartialUpdateRequest
{
    public int TemplateId { get; set; }
    public int Start { get; set; }
    public int End { get; set; }
}