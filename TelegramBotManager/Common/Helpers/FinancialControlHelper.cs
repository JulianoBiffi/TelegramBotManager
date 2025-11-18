using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotManager.Application.FinancialControl.FinanceControlCreateTransaction;
using TelegramBotManager.Domain.Entities.FinancialControl;

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
                    /*new[]
                    {
                        InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(
                            "    Listagem de lançamentos    ",
                            "\n/listagem\n" +
                            "Data (vázio para o mês atual):"
                        ),
                    },*/
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(
                            "    Listagem de lançamentos do mês atual    ",
                            "\n/relatoriomensal\n"
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
        var message = new StringBuilder();
        message.AppendLine("✅ *Lançamento cadastrado com sucesso!*");
        message.AppendLine($"📅 *Data:* {result.Date:dd/MM/yyyy}");
        message.AppendLine($"💳 *Cartão:* {result.CreditCard}");
        message.AppendLine($"💰 *Valor:* R$ {result.Value:N2}");
        message.AppendLine($"📝 *Descrição:* {result.Description}");

        if (!string.IsNullOrEmpty(result.Category?.Description))
        {
            message.AppendLine($"🏷️ *Categoria:* {result.Category.Description}");
        }

        if (result.ParcelNumber.HasValue)
        {
            message.AppendLine($"🔄 *Número da parcela:* {result.ParcelNumber}");
        }

        message.AppendLine("━━━━━━━━━━━━━━━━━━━━");
        message.AppendLine($"📊 *Total do mês nesse cartão:* R$ {result.AmmountOfMonth:N2}");
        message.Append($"📈 *Total da categoria:* R$ {result.AmmountOfThisCategory:N2}");

        await telegramBotClient.SendMessage(
            chatId,
            message.ToString(),
            parseMode: ParseMode.Markdown);
    }

    public static async Task PrintCategorys(
        this TelegramBotClient telegramBotClient,
        long chatId,
        long transactionId,
        List<category> listOfCategorys)
    {
        var message = new StringBuilder();
        message.AppendLine();

        var allButtons = new List<InlineKeyboardButton>();
        listOfCategorys.ForEach(category =>
        {
            allButtons.Add(
                InlineKeyboardButton.WithCallbackData(
                        category.Description,
                        $"\n/definircategoria&transactionId={transactionId}&categoryId={category.Id}\n"
                        ));
        });

        var inlineKeyboard =
           new InlineKeyboardMarkup(
               new[]
               {
                   allButtons.ToArray(),
                   new[]
                   {
                       InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(
                            "    Cadastro de nova categoria    ",
                            $"\n/cadastrarcategoria&transactionId={transactionId}\n" +
                            "Descrição: "
                        ),
                   }
               });

        await telegramBotClient.SendMessage(
            chatId, "🔄 *Vincule este lançamento a uma categoria!*",
            replyMarkup: inlineKeyboard);
    }

    private static InlineKeyboardButton CreateTransaction()
    {
        return
            InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(
                            "    Cadastro de lançamento    ",
                            "\n/cadastro\n" +
                            "Data (vázio para o dia atual ou insira um intervalo): \n" +
                            "Cartão (bb, nu, porto, va): \n" +
                            "Valor: \n" +
                            "Descrição da compra: \n" +
                            "Parcelas (vazio se não for parcelado): "
                        );
    }
}
