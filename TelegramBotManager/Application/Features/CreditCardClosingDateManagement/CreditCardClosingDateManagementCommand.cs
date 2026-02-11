using MediatR;

namespace TelegramBotManager.Application.Features.CreditCardClosingDateManagement;

public class CreditCardClosingDateManagementCommand : IRequest<Unit>
{
    public string MessageBody { get; set; } = string.Empty;
}
