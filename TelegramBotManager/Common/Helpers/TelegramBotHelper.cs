using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace TelegramBotManager.Common.Helpers;

public static class TelegramBotHelper
{
    public static async Task SendMessage(this TelegramBotClient telegramBotClient, string chatId, string message)
        => await telegramBotClient.SendMessage(chatId, message);

    public static async Task SendTyping(this TelegramBotClient telegramBotClient, string chatId)
        => await telegramBotClient.SendChatAction(chatId: chatId, action: ChatAction.Typing);
}