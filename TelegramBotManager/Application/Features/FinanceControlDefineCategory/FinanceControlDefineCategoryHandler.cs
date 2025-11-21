using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TelegramBotManager.Common.Helpers;
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
        if (!request.CategoryId.HasValue)
        {
            var listOfCategory =
                await _CategoryRepository.GetAllCategoriesAsync(cancellationToken);

            await _telegramBotClient.PrintCategorys(
                _financialControlOptions.AllowedGroup,
                request.TransactionId,
                listOfCategory);

            return Unit.Value;
        }
        var transaction =
            await _TransactionRepository.GetTransactionById(request.TransactionId, cancellationToken);

        transaction.CategoryId = request.CategoryId;

        await _TransactionRepository.SaveAsync(transaction, cancellationToken);

        var category =
            await _CategoryRepository.GetCategory(request.CategoryId.Value, cancellationToken);

        var ammountOfThisCategory =
            await _TransactionRepository.GetAmmountOfMonth(transaction, cancellationToken, true);

        await _telegramBotClient.PrintNewCategory(
            _financialControlOptions.AllowedGroup,
            transaction.Description,
            category.Description,
            ammountOfThisCategory);

        return Unit.Value;
    }
}
