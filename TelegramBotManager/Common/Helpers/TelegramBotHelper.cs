using Telegram.Bot;

namespace TelegramBotManager.Common.Helpers;

public static class TelegramBotHelper
{
    public static async Task SendMessage(this TelegramBotClient telegramBotClient, string chatId, string message)
        => await telegramBotClient.SendMessage(chatId, message);
}