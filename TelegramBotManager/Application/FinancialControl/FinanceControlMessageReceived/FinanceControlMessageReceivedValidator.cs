using FluentValidation;

namespace TelegramBotManager.Application.FinancialControl.FinanceControlMessageReceived;

public class FinanceControlMessageReceivedValidator : AbstractValidator<FinanceControlMessageReceivedCommand>
{
    public FinanceControlMessageReceivedValidator()
    {
        RuleFor(x => x.Request).NotNull().WithMessage("Request cannot be null");
    }
}
