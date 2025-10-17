namespace TelegramBotManager.Configurations;

public class FinancialControlOptions
{
    public string TelegramBotToken { get; private set; }
    public long[] AllowedUserIds { get; private set; }
    public long AllowedGroup { get; private set; }

    public FinancialControlOptions(
        string telegramBotToken,
        long[] allowedUserIds,
        long allowedGroup)
    {
        TelegramBotToken = telegramBotToken;
        AllowedUserIds = allowedUserIds;
        AllowedGroup = allowedGroup;
    }
}
