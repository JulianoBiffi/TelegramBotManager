using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotManager.Application.FinancialControl.FinanceControlCreateTransaction;

namespace TelegramBotManager.Common.Helpers;

public static class FinancialControlHelper
{
    public static InlineKeyboardMarkup ListOfOptions()
    {
        var inlineKeyboard =
            new InlineKeyboardMarkup(
                new[]
                {
                    new[]
                    {
                        CreateTransaction(),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(
                            "    Listagem de lançamentos    ",
                            "\n/listagem\n" +
                            "Data (vázio para o mês atual):"
                        ),
                    }
                });

        return inlineKeyboard;
    }

    public static async Task PrintListOfOptions(
        this TelegramBotClient telegramBotClient,
        string chatId)
        => await telegramBotClient.SendMessage(chatId, "Opções inválida!", replyMarkup: ListOfOptions());

    public static async Task PrintCreateTransaction(
        this TelegramBotClient telegramBotClient,
        long chatId,
        string message)
    {
        await telegramBotClient.SendMessage(
            chatId,
            message,
            replyMarkup:
                new InlineKeyboardMarkup(
                    new[]
                    {
                        new[]
                        {
                            CreateTransaction(),
                        }
                    }));
    }

    public static async Task PrintCreatedTransaction(
        this TelegramBotClient telegramBotClient,
        long chatId,
        FinanceControlCreateTransactionResult result)
    {
        var message = $"""
        ✅ *Lançamento cadastrado com sucesso!*
        📅 *Data:* {result.Date:dd/MM/yyyy}
        💳 *Cartão:* {result.CreditCard}
        💰 *Valor:* R$ {result.Value:N2}
        📝 *Descrição:* {result.Description}
        🏷️ *Categoria:* {result.Category.Description}
        ━━━━━━━━━━━━━━━━━━━━
        📊 *Total do mês:* R$ {result.AmmountOfMonth:N2}
        📈 *Total da categoria:* R$ {result.AmmountOfThisCategory:N2}
        """;

        await telegramBotClient.SendMessage(
            chatId,
            message,
            parseMode: ParseMode.Markdown);
    }

    private static InlineKeyboardButton CreateTransaction()
    {
        return
            InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(
                            "    Cadastro de lançamento    ",
                            "\n/cadastro\n" +
                            "Data (vázio para o dia atual ou insira um intervalo):\n" +
                            "Cartão (bb, nu, porto, va):\n" +
                            "Valor:\n" +
                            "Descrição da compra:"
                        );
    }
}
