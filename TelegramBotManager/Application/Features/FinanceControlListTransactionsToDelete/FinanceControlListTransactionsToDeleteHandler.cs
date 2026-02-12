using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotManager.Common.Helpers;
using TelegramBotManager.Configurations;
using TelegramBotManager.Domain.Interfaces;

namespace TelegramBotManager.Application.Features.FinanceControlListTransactionsToDelete;

public class FinanceControlListTransactionsToDeleteHandler(
    ITransactionRepository _TransactionRepository,
    FinancialControlOptions _financialControlOptions,
    [FromKeyedServices("FinancialControl")] TelegramBotClient _telegramBotClient) : IRequestHandler<FinanceControlListTransactionsToDeleteCommand, Unit>
{
    public async Task<Unit> Handle(
        FinanceControlListTransactionsToDeleteCommand request,
        CancellationToken cancellationToken)
    {
        var allTransactionsFromMonth =
            await _TransactionRepository.GetTransactionsByPeriod(
                DateTimeHelper.GetFirstDayOfThisMonth(),
                cancellationToken);

        allTransactionsFromMonth =
            allTransactionsFromMonth.OrderBy(x => x.Date)
                                    .ToList();

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
                        $"📅 {t.Date:dd/MM} | {descriptionNormalized} | R$ {t.Value:N2}",
                        $"\n/deletartransacao&transactionid={t.Id}\n"
                        ),
                });
        });

        var inlineKeyboard =
           new InlineKeyboardMarkup(allButtons.ToArray());

        await _telegramBotClient.SendMessage(
            _financialControlOptions.AllowedGroup,
            "Selecione o lançamento que deseja excluir:",
            replyMarkup: inlineKeyboard);

        return Unit.Value;
    }
}
