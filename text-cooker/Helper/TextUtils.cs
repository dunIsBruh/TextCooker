namespace text_cooker.Helper;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class TextUtils
{
    private static readonly Regex MultiSpaceRe = new(@"\s{2,}", RegexOptions.Compiled);
    private static readonly Regex SentenceEndRe = new(
        @"([.!?])\s+([a-zа-яё])", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
        );

    public static string NormalizeWhitespace(string text)
        => MultiSpaceRe.Replace(text.Trim(), " ");

    public static string NormalizePunctuation(string text)
    {
        // simple normalization: replace multiple punctuation signs with single
        text = Regex.Replace(text, @"[ ]+([.,!?:;])", "$1");
        text = Regex.Replace(text, @"([!?]{2,})", m => m.Value[0].ToString());
        return text;
    }

    public static string CapitalizeSentences(string text)
    {
        // very simple sentence capitalization: make first letter after sentence end uppercase
        return SentenceEndRe.Replace(text, m =>
        {
            var end = m.Groups[1].Value;
            var next = m.Groups[2].Value.ToUpper();
            return $"{end} {next}";
        });
    }

    public static List<string> SplitIntoParagraphs(string text)
    {
        // paragraphs: split on double newline or single newline followed by indentation
        var parts = Regex.Split(text.Trim(), @"\r?\n\s*\r?\n")
                         .Select(p => p.Trim())
                         .Where(p => !string.IsNullOrEmpty(p))
                         .ToList();
        return parts;
    }

    public static List<string> TokenizeWords(string text)
    {
        var cleaned = Regex.Replace(text.ToLowerInvariant(), @"[^\p{L}\d\s]", " ");
        var tokens = cleaned.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        return tokens.ToList();
    }

    public static string ReplacePlaceholders(string templateText, Dictionary<string, string>? values)
    {
        if (string.IsNullOrEmpty(templateText) || values == null || values.Count == 0)
            return templateText;

        string result = templateText;
        foreach (var kv in values)
        {
            result = result.Replace("{" + kv.Key + "}", kv.Value, StringComparison.InvariantCultureIgnoreCase);
        }
        return result;
    }
}
