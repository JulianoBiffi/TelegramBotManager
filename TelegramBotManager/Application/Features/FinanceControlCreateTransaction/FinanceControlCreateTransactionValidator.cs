using FluentValidation;

namespace TelegramBotManager.Application.FinancialControl.FinanceControlCreateTransaction;

public class FinanceControlCreateTransactionValidator : AbstractValidator<FinanceControlCreateTransactionCommand>
{
    public FinanceControlCreateTransactionValidator()
    {
        RuleFor(x => x.MessageBody).NotNull().WithMessage("Request cannot be null");
    }
}
