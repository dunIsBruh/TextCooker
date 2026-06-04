namespace text_cooker.Helper.EnginesUtils;

public static class SimplePromptBuilder
{
    public static string BuildPrompt(string template, string text, double strength)
    {
        return "USER:\n" +
               "перепиши фрагмент для переписывания под шаблон" +
               "Вот шаблон (идеальный пример). Проанализируйте его тон, длину предложений, структуру, стиль пунктуации и словарный состав:\n" +
               "---\n" +
               $"{template}\n" +
               "---\n\n" +
               "Вот фрагмент для переписывания:\n" +
               "---\n" +
               $"{text}\n" + 
               $"выведи ТОЛЬКО переписанный фрагмент текста";
    }
}