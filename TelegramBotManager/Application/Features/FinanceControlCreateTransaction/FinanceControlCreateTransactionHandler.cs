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
                    Description = currentDto.Description + (currentDto.ParcelNumber.HasValue ? $" (Parcelado em {currentDto.ParcelNumber})" : string.Empty),
                    Date = currentDto.Date,
                    Value = currentDto.Value,
                    CategoryId = tryToFindCategory?.Id,
                    ParcelNumber = currentDto.ParcelNumber.HasValue ? 1 : null
                },
                cancellationToken);

        if (currentDto.ParcelNumber.HasValue)
        {
            //TODO: Should consider the credit card closing date !!

            for (int parcelNumber = 1; parcelNumber < currentDto.ParcelNumber.Value; parcelNumber++)
            {
                var parcelDate =
                    currentDto.Date.AddMonths(parcelNumber);

                await _TransactionRepository.SaveAsync(
                    new Domain.Entities.FinancialControl.transaction()
                    {
                        CreditCard = savedTransaction.CreditCard,
                        Description = currentDto.Description + (currentDto.ParcelNumber.HasValue ? $" (Parcela {parcelNumber + 1}/{currentDto.ParcelNumber})" : string.Empty),
                        Date = parcelDate,
                        Value = savedTransaction.Value,
                        CategoryId = savedTransaction.CategoryId,
                        ParcelNumber = parcelNumber + 1
                    },
                    cancellationToken);
            }
        }

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
