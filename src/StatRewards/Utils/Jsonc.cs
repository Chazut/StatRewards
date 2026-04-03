using System.Text.RegularExpressions;

namespace StatRewards.Utils;

public static class Jsonc
{
    private static readonly Regex MultiLineComment = new(@"/\*.*?\*/", RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex SingleLineComment = new(@"//.*?$", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex TrailingCommas = new(@",(\s*[}\]])", RegexOptions.Compiled);

    public static string Strip(string jsonc)
    {
        if (string.IsNullOrWhiteSpace(jsonc)) return "{}";
        var noComments = MultiLineComment.Replace(jsonc, string.Empty);
        noComments = SingleLineComment.Replace(noComments, string.Empty);
        var noTrailing = TrailingCommas.Replace(noComments, "$1");
        return noTrailing;
    }
}