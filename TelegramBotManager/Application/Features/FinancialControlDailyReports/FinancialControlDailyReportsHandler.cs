using Azure.Storage.Blobs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotManager.Common.Helpers;
using TelegramBotManager.Configurations;
using TelegramBotManager.Domain.Interfaces;

namespace TelegramBotManager.Application.Features.FinancialControlDailyReports;

public class FinancialControlDailyReportsHandler(
    ITransactionRepository _TransactionRepository,
    FinancialControlOptions _financialControlOptions,
    [FromKeyedServices("FinancialControl")] TelegramBotClient _telegramBotClient) : IRequestHandler<FinancialControlDailyReportsCommand, Unit>
{
    public async Task<Unit> Handle(
        FinancialControlDailyReportsCommand request,
        CancellationToken cancellationToken)
    {
        var allTransactionsFromMonth =
            await _TransactionRepository.GetTransactionsByPeriod(
                DateTimeHelper.GetFirstDayOfThisMonth(),
                DateTimeHelper.GetLastDayOfThisMonth(),
                cancellationToken);

        var transactionMessage =
            new StringBuilder()
                .AppendLine("🏦 *Informe diário de movimentações*");

        if (!allTransactionsFromMonth.Any())
        {
            transactionMessage.AppendLine("📝 Nenhuma transação registrada até o momento neste mês!.");
        }

        var transactionsGroupedByCreditCard =
            allTransactionsFromMonth
            .Where(x => !string.IsNullOrEmpty(x.CreditCard))
            .GroupBy(t => t.CreditCard)
            .OrderBy(g => g.Key)
            .Select(g => new { g.Key, SumOfValues = g.Sum(t => t.Value) });

        transactionMessage.AppendLine($"━━━━━━━━━━━━━━━━━━━━━━━");
        foreach (var groupOfTransaction in transactionsGroupedByCreditCard)
        {
            transactionMessage.AppendLine($"💳 Resumo das transações do cartão: *{groupOfTransaction.Key}*");
            transactionMessage.AppendLine($"💰 Total gasto: R$ {groupOfTransaction.SumOfValues:N2}");
            transactionMessage.AppendLine(string.Empty);
        }

        if (allTransactionsFromMonth.Any())
        {
            transactionMessage.AppendLine($"━━━━━━━━━━━━━━━━━━━━━━━");
            transactionMessage.AppendLine($"💸 Total gasto: *R$ {allTransactionsFromMonth.Sum(y => y.Value):N2}*");

            var transcationWithCategory =
                allTransactionsFromMonth
                .Where(t => t.Category != null)
                .Select(g => new Tuple<string, double>(g.Category.Description, (double)g.Value))
                .ToList();

            var pieChartPath =
                await ScottPlotHelper.GeneratePieChart(transcationWithCategory);

            using var fileStream = System.IO.File.OpenRead(pieChartPath);

            await _telegramBotClient.SendPhoto(
                chatId: _financialControlOptions.AllowedGroup,
                photo: new InputFileStream(fileStream),
                caption: transactionMessage.ToString(),
                parseMode: ParseMode.Markdown
            );

            await FileHelper.SafeDeleteFile(pieChartPath);

            return Unit.Value;
        }

        await _telegramBotClient.SendMessage(
            _financialControlOptions.AllowedGroup,
            transactionMessage.ToString(),
            parseMode: ParseMode.Markdown);

        return Unit.Value;
    }
}
