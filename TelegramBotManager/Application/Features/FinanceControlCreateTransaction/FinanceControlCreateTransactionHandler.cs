using FluentValidation;
using MediatR;
using TelegramBotManager.Application.Mappers;
using TelegramBotManager.Domain.Interfaces;

namespace TelegramBotManager.Application.FinancialControl.FinanceControlCreateTransaction;

public class FinanceControlCreateTransactionHandler(
    ITransactionRepository _TransactionRepository,
    ICategoryRepository _CategoryRepository) : IRequestHandler<FinanceControlCreateTransactionCommand, FinanceControlCreateTransactionResult>
{
    public async Task<FinanceControlCreateTransactionResult> Handle(
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

        var savedTransaction =
            await _TransactionRepository.SaveAsync(
                new Domain.Entities.FinancialControl.transaction()
                {
                    CreditCard = currentDto.CreditCard,
                    Description = currentDto.Description,
                    Date = currentDto.Date,
                    Value = currentDto.Value,
                    CategoryId = tryToFindCategory?.Id
                },
                cancellationToken);

        var ammountOfMonth =
            await _TransactionRepository.GetAmmountOfMonth(savedTransaction, cancellationToken);

        var ammountOfThisCategory =
            await _TransactionRepository.GetAmmountOfMonth(savedTransaction, cancellationToken, true);

        return
            new FinanceControlCreateTransactionResult()
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
    }
}
