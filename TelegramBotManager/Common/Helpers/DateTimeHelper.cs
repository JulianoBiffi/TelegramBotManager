using System.Globalization;

namespace TelegramBotManager.Common.Helpers;

public static class DateTimeHelper
{
    public static DateTime TryTakeDate(this string dateText)
    {
        if (string.IsNullOrEmpty(dateText))
            return BrazilNow;

        if (dateText.Length == 1)
        {
            return DateTime.Parse($"0{dateText}/{BrazilNow.Month}/{BrazilNow.Year}", CultureInfo.GetCultureInfo("pt-BR"));
        }

        if (dateText.Length == 2)
        {
            return DateTime.Parse($"{dateText}/{BrazilNow.Month}/{BrazilNow.Year}", CultureInfo.GetCultureInfo("pt-BR"));
        }

        if (dateText.Length == 5)
        {
            return DateTime.Parse($"{dateText}/{BrazilNow.Year}", CultureInfo.GetCultureInfo("pt-BR"));
        }

        if (dateText.Contains("/") && dateText.Length == 10)
        {
            return
                DateTime.TryParse(dateText, CultureInfo.GetCultureInfo("pt-BR"), DateTimeStyles.None, out DateTime date)
                    ? date
                    : BrazilNow;
        }

        return BrazilNow;
    }

    public static DateTime GetFirstDayOfThisMonth()
    {
        var now = BrazilNow;
        return new DateTime(now.Year, now.Month, 1);
    }

    public static DateTime GetLastDayOfThisMonth()
    {
        var now = BrazilNow;
        var firstDayOfNextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);
        return firstDayOfNextMonth.AddDays(-1);
    }

    /// <summary>
    /// Gets current date and time with brazil convertion
    /// </summary>
    public static DateTime BrazilNow
    {
        get
        {
            return DateTime.Now.ToBrazilTime();
        }
    }

    /// <summary>
    /// Convert current <paramref name="date"/> to brazilian date and time.
    /// </summary>
    /// <param name="date">Date which you wanna convert</param>
    /// <returns><see cref="DateTime"/> converted.</returns>
    public static DateTime ToBrazilTime(this DateTime date)
    {
        var brazilTimeZoneInfo =
            TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

        return TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Local, brazilTimeZoneInfo);
    }
}