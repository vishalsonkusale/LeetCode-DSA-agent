using System.Text;
using System.Text.RegularExpressions;
using System.Net;

namespace LeetCodeAgent;

public static partial class Utilities
{
    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);

        foreach (var character in value.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
            else if (char.IsWhiteSpace(character) || character is '\'' or '-' or '_')
            {
                builder.Append(' ');
            }
        }

        return WhitespaceRegex().Replace(builder.ToString(), " ").Trim();
    }

    public static int? ExtractProblemId(string input)
    {
        var match = ProblemIdRegex().Match(input);
        return match.Success && int.TryParse(match.Groups["id"].Value, out var id) ? id : null;
    }

    public static bool ContainsAny(string input, params string[] terms)
    {
        var normalizedInput = Normalize(input);
        return terms.Any(term => normalizedInput.Contains(Normalize(term)));
    }

    public static bool IsExitCommand(string input)
    {
        var normalizedInput = Normalize(input);
        return normalizedInput is "exit" or "quit" or "q";
    }

    public static string? ExtractLeetCodeSlug(string input)
    {
        var match = LeetCodeProblemUrlRegex().Match(input);
        return match.Success ? match.Groups["slug"].Value : null;
    }

    public static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var withLineBreaks = html
            .Replace("</p>", Environment.NewLine, StringComparison.OrdinalIgnoreCase)
            .Replace("<br>", Environment.NewLine, StringComparison.OrdinalIgnoreCase)
            .Replace("<br/>", Environment.NewLine, StringComparison.OrdinalIgnoreCase)
            .Replace("<br />", Environment.NewLine, StringComparison.OrdinalIgnoreCase)
            .Replace("</pre>", Environment.NewLine, StringComparison.OrdinalIgnoreCase);

        var withoutTags = HtmlTagRegex().Replace(withLineBreaks, string.Empty);
        var decoded = WebUtility.HtmlDecode(withoutTags);

        return WhitespaceWithNewLinesRegex().Replace(decoded, Environment.NewLine).Trim();
    }

    public static string Bullets(IEnumerable<string> items) =>
        string.Join(Environment.NewLine, items.Select(item => $"- {item}"));

    [GeneratedRegex(@"leetcode\.com/problems/(?<slug>[a-z0-9-]+)", RegexOptions.IgnoreCase)]
    private static partial Regex LeetCodeProblemUrlRegex();

    [GeneratedRegex(@"(?:problem|leetcode|lc|#)?\s*(?<id>\d{1,4})", RegexOptions.IgnoreCase)]
    private static partial Regex ProblemIdRegex();

    [GeneratedRegex("<.*?>", RegexOptions.Singleline)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"(?:\r?\n\s*){3,}")]
    private static partial Regex WhitespaceWithNewLinesRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
