using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotManager.Common.Helpers;

public static class FinancialControlHelper
{
    public static InlineKeyboardMarkup ListOfOptions()
    {
        var inlineKeyboard =
            new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(
                    "Cadastro de lançamento         ",
                    "\n/cadastro\n" +
                    "Data (vázio para o dia atual):\n" +
                    "Cartão (bb, nu, porto, va):\n" +
                    "Valor:\n" +
                    "Categoria:"
                ),
                InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(
                    "Listagem de lançamentos         ",
                    "\n/listagem\n" +
                    "Data (vázio para o mês atual):"
                ),
            });

        return inlineKeyboard;
    }

    public static async Task PrintListOfOptions(
        this TelegramBotClient telegramBotClient,
        string chatId)
        => await telegramBotClient.SendMessage(chatId, "Opções inválida!", replyMarkup: ListOfOptions());
}
