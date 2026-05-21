using System.Text.RegularExpressions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotManager.Common.Helpers;
using TelegramBotManager.Configurations;
using TelegramBotManager.Domain.Interfaces;

namespace TelegramBotManager.Application.Features.FinanceControlEditTransactionsOfMonth;

public class FinanceControlEditTransactionsOfMonthHandler(
    ITransactionRepository _TransactionRepository,
    ICategoryRepository _CategoryRepository,
    FinancialControlOptions _financialControlOptions,
    [FromKeyedServices("FinancialControl")] TelegramBotClient _telegramBotClient) : IRequestHandler<FinanceControlEditTransactionsOfMonthCommand, Unit>
{
    public async Task<Unit> Handle(
        FinanceControlEditTransactionsOfMonthCommand request,
        CancellationToken cancellationToken)
    {
        var allTransactionsFromMonth =
            await _TransactionRepository.GetTransactionsByPeriod(
                DateTimeHelper.GetFirstDayOfThisMonth(),
                cancellationToken);

        var listOfCategories =
            await _CategoryRepository.GetAllCategoriesAsync(cancellationToken);

        var allButtons = new List<InlineKeyboardButton[]>();
        allTransactionsFromMonth.ForEach(t =>
        {

            var descriptionNormalized = Regex.Replace(t.Description, @"\s+", " ").Trim();

            if (descriptionNormalized.Length > 20)
                descriptionNormalized = descriptionNormalized.Substring(0, 17) + "...";

            allButtons.Add(
                 new[]
                 {
                     InlineKeyboardButton.WithCallbackData(
                         $"🔄 {t.Date:dd/MM} | {descriptionNormalized} | R$ {t.Value:N2}",
                        $"\n/definircategoria&transactionid={t.Id}\n"
                        ),
                });
        });

        var inlineKeyboard =
           new InlineKeyboardMarkup(allButtons.ToArray());

        await _telegramBotClient.SendMessage(
            _financialControlOptions.AllowedGroup,
            "Selecione o lançamento que deseja editar:",
            replyMarkup: inlineKeyboard);

        return Unit.Value;
    }
}
