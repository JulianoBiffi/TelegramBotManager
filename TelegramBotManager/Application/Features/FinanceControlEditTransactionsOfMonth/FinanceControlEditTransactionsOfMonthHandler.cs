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
            var description =
                $"{t.Description}\n" +
                $"R$ {t.Value:N2}\n" +
                (t.CategoryId.HasValue ? $"Categoria: {listOfCategories.FirstOrDefault(c => c.Id == t.CategoryId)?.Description ?? string.Empty}" : "");

            allButtons.Add(
                 new[]
                 {
                     InlineKeyboardButton.WithCallbackData(
                        description,
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
