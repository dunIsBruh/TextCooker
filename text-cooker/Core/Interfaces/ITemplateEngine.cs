using text_cooker.Entities;
using text_cooker.Models;

namespace text_cooker.Core.Interfaces;

public interface ITemplateEngine
{
    Task<string> ApplyToText(Template template, string text);

    Task<string> RewriteByTemplate(string templateText, string userText);
}