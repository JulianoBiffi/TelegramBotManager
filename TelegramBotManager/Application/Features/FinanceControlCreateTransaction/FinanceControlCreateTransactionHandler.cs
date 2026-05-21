using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TelegramBotManager.Application.Features.FinanceControlDefineCategory;
using TelegramBotManager.Application.Mappers;
using TelegramBotManager.Common.Helpers;
using TelegramBotManager.Configurations;
using TelegramBotManager.Domain.Entities.FinancialControl;
using TelegramBotManager.Domain.Interfaces;
using TelegramBotManager.Domain.Services;

namespace TelegramBotManager.Application.FinancialControl.FinanceControlCreateTransaction;

public class FinanceControlCreateTransactionHandler(
    ITransactionRepository _TransactionRepository,
    ICategoryRepository _CategoryRepository,
    IMediator _mediator,
    FinancialControlOptions _financialControlOptions,
    [FromKeyedServices("FinancialControl")] TelegramBotClient _telegramBotClient) : IRequestHandler<FinanceControlCreateTransactionCommand, Unit>
{
    public async Task<Unit> Handle(
        FinanceControlCreateTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var validator =
            new FinanceControlCreateTransactionValidator();

        var validationResult =
            await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var currentDto =
            FinancialControlCreateTransactionMapper.ToDto(command.MessageBody);

        var tryToFindCategory =
            await _CategoryRepository.GetCategoryByTranscationDesciption(currentDto.Description);

        var transactions = TransactionInstallmentService.CreateInstallments(
            currentDto.Description,
            currentDto.Value,
            currentDto.Date,
            currentDto.CreditCard,
            tryToFindCategory,
            currentDto.ParcelNumber);

        Transaction savedTransaction = null;

        foreach (var transaction in transactions)
        {
            var saved = await _TransactionRepository.SaveAsync(transaction, cancellationToken);
            if (savedTransaction == null)
            {
                savedTransaction = saved;
            }
        }

        var firstDay = DateTimeHelper.GetFirstDayOfThisMonth();
        var lastDay = DateTimeHelper.GetLastDayOfThisMonth();

        var ammountOfMonth =
            await _TransactionRepository.GetAmountByPeriodAsync(firstDay, lastDay, null, cancellationToken);

        var ammountOfThisCategory =
            await _TransactionRepository.GetAmountByPeriodAsync(firstDay, lastDay, savedTransaction.CategoryId, cancellationToken, true);

        var result = new FinanceControlCreateTransactionResult()
        {
            Description = savedTransaction.Description,
            CreditCard = savedTransaction.CreditCard,
            Date = savedTransaction.Date,
            Id = savedTransaction.Id,
            Value = savedTransaction.Value,
            Category = tryToFindCategory,
            AmmountOfMonth = ammountOfMonth,
            AmmountOfThisCategory = ammountOfThisCategory
        };

        await _telegramBotClient.PrintCreatedTransaction(_financialControlOptions.AllowedGroup, result);

        if (result.Category == null)
        {
            await _mediator.Send(new FinanceControlDefineCategoryCommand(result.Id), cancellationToken);
        }

        return Unit.Value;
    }
}
