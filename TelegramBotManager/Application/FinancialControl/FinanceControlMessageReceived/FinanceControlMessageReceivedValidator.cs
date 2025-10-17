using FluentValidation;

namespace TelegramBotManager.Application.FinancialControl.FinanceControlMessageReceived;

public class FinanceControlMessageReceivedValidator : AbstractValidator<FinanceControlMessageReceivedCommand>
{
    public FinanceControlMessageReceivedValidator()
    {
        RuleFor(x => x.Request).NotNull().WithMessage("Request cannot be null");
        RuleFor(x => x.Request.Message).NotNull().WithMessage("Message cannot be null");
        RuleFor(x => x.Request.Message.Text).NotNull().WithMessage("Message Text cannot be null");
    }
}
