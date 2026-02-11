using System.Text.RegularExpressions;

namespace TelegramBotManager.Common.Helpers;

public static class StringHelper
{
    public static long? TryTakeValueFromString(this string source, string fieldname)
    {
        var transactionMatch =
            Regex.Match(source.ToLower(), $@"{fieldname.ToLower()}=(\d+)", RegexOptions.IgnoreCase);

        return
            long.TryParse(transactionMatch.Groups[1].Value, out long value)
            ? value
            : null;
    }

    public static string? TryTakeStringValueFromString(this string source, string fieldname)
    {
        var match =
            Regex.Match(source.ToLower(), $@"{fieldname.ToLower()}=([a-z0-9_-]+)", RegexOptions.IgnoreCase);

        return match.Success ? match.Groups[1].Value : null;
    }
}
