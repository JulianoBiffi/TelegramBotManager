using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TelegramBotManager.Common.Exceptions;
using TelegramBotManager.Common.Helpers;
using TelegramBotManager.Configurations;
using TelegramBotManager.Domain.Entities.FinancialControl;
using TelegramBotManager.Domain.Interfaces;

namespace TelegramBotManager.Application.Features.FinanceControlCreateCategory;

public class FinanceControlCreateCategoryHandler(
    ICategoryRepository _CategoryRepository,
    ITransactionRepository _TransactionRepository,
    FinancialControlOptions _financialControlOptions,
    [FromKeyedServices("FinancialControl")] TelegramBotClient _telegramBotClient) : IRequestHandler<FinanceControlCreateCategoryCommand, Unit>
{
    public async Task<Unit> Handle(
        FinanceControlCreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var chunckOfLines =
            request.MessageBody.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(l => l.Trim())
                       .ToArray();

        var transactionIdText =
            chunckOfLines[0].TryTakeValueFromString("transactionid");

        if (!transactionIdText.HasValue)
            throw new TelegramException($"Não foi possível identificar a transação no comando: {chunckOfLines[0]}");

        var categoryDescription =
            chunckOfLines[1].Split(':')[1].Trim();

        var savedCategory =
            await _CategoryRepository.SaveAsync(
                new Category(categoryDescription),
                cancellationToken);

        var transaction =
            await _TransactionRepository.GetTransactionById(
                transactionIdText.Value,
                cancellationToken);

        transaction.SetCategory(savedCategory);

        await _TransactionRepository.SaveAsync(transaction, cancellationToken);

        var ammountOfThisCategory =
            await _TransactionRepository.GetAmmountOfMonth(transaction, cancellationToken, true);

        await _telegramBotClient.PrintNewCategory(
            _financialControlOptions.AllowedGroup,
            transaction.Description,
            savedCategory.Description,
            ammountOfThisCategory);

        return Unit.Value;
    }
}
