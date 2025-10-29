using FluentValidation;

namespace TelegramBotManager.Application.FinancialControl.FinanceControlMessageReceived;

public class FinanceControlMessageReceivedValidator : AbstractValidator<FinanceControlMessageReceivedCommand>
{
    public FinanceControlMessageReceivedValidator()
    {
        RuleFor(x => x.Request).NotNull().WithMessage("Request cannot be null");
        RuleFor(x => x.Request.Message).NotNull().WithMessage("Message cannot be null");
        RuleFor(x => x.Request.Message.Text).NotNull().NotEmpty().WithMessage("Message Text cannot be null");
        RuleFor(x => x.Request.Message.From).NotNull().WithMessage("From cannot be null");
        RuleFor(x => x.Request.Message.From.Id).NotNull().NotEmpty().WithMessage("From.Id cannot be null");
    }
}
