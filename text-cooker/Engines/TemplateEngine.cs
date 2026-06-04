using text_cooker.Core.Interfaces;
using text_cooker.Entities;
using text_cooker.Helper.EnginesUtils;

namespace text_cooker.Engines;

public class TemplateEngine(IEmbeddingService embeddings, IParaphraseService paraphraser)
    : ITemplateEngine
{
    public async Task<string> ApplyToText(Template template, string text)
    {
        return await RewriteByTemplate(template.Text, text);
    }

    public async Task<string> RewriteByTemplate(string templateText, string userText)
    {
        // 1. embeddings
        var templateEmb = await embeddings.GetEmbeddingAsync(templateText);
        var textEmb = await embeddings.GetEmbeddingAsync(userText);

        if (templateEmb == null || textEmb == null)
            return userText;

        // 2. similarity
        var styleStrength = ParamsCalculator.StyleStrength(templateEmb, textEmb);

        // 3. paraphrase using styleStrength
        string request = SimplePromptBuilder.BuildPrompt(templateText, userText, styleStrength);

        var rewritten = await paraphraser.ParaphraseToStyleAsync(request);

        return rewritten ?? userText;
    }
}
