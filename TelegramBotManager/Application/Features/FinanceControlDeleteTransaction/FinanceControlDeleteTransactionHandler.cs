using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramBotManager.Configurations;
using TelegramBotManager.Domain.Interfaces;

namespace TelegramBotManager.Application.Features.FinanceControlDeleteTransaction;

public class FinanceControlDeleteTransactionHandler(
    ITransactionRepository _TransactionRepository,
    FinancialControlOptions _financialControlOptions,
    [FromKeyedServices("FinancialControl")] TelegramBotClient _telegramBotClient) : IRequestHandler<FinanceControlDeleteTransactionCommand, Unit>
{
    public async Task<Unit> Handle(
        FinanceControlDeleteTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var transaction =
            await _TransactionRepository.GetTransactionById(
                request.TransactionId,
                cancellationToken);

        if (transaction != null)
        {
            await _TransactionRepository.DeleteAsync(transaction.Id, cancellationToken);

            await _telegramBotClient.SendMessage(
               _financialControlOptions.AllowedGroup,
               $"🗑️ *Lançamento deletado com sucesso!*\n\n📝 {transaction.Description}\n💰 R$ {transaction.Value:N2}\n📅 {transaction.Date:dd/MM/yyyy}",
               parseMode: ParseMode.Markdown);
        }
        else
        {
            await _telegramBotClient.SendMessage(
               _financialControlOptions.AllowedGroup,
               $"⚠️ *Lançamento não encontrado ou já deletado!*",
               parseMode: ParseMode.Markdown);
        }

        return Unit.Value;
    }
}
