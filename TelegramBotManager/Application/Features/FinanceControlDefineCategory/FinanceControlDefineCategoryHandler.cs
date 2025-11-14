using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TelegramBotManager.Configurations;
using TelegramBotManager.Domain.Interfaces;

namespace TelegramBotManager.Application.Features.FinanceControlDefineCategory;

public class FinanceControlDefineCategoryHandler(
    ITransactionRepository _TransactionRepository,
    ICategoryRepository _CategoryRepository,
    FinancialControlOptions _financialControlOptions,
    [FromKeyedServices("FinancialControl")] TelegramBotClient _telegramBotClient) : IRequestHandler<FinanceControlDefineCategoryCommand, Unit>
{
    public async Task<Unit> Handle(
        FinanceControlDefineCategoryCommand request,
        CancellationToken cancellationToken)
    {

        return Unit.Value;
    }
}
