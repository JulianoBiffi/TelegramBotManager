using FluentValidation;
using MediatR;
using TelegramBotManager.Application.Mappers;
using TelegramBotManager.Domain.Interfaces;
using TelegramBotManager.Domain.Entities.FinancialControl;

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

        // Domain Logic: Create transactions (including logic for parcels)
        var transactions = Transaction.CreateInstallments(
            currentDto.Description,
            currentDto.Value,
            currentDto.Date,
            currentDto.CreditCard,
            tryToFindCategory,
            currentDto.ParcelNumber);

        Transaction savedTransaction = null;

        // Persist all generated transactions
        foreach (var transaction in transactions)
        {
            var saved = await _TransactionRepository.SaveAsync(transaction, cancellationToken);
            if (savedTransaction == null) 
            {
                savedTransaction = saved;
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
