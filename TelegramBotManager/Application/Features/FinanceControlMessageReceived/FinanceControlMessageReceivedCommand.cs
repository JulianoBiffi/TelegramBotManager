using MediatR;
using TelegramBotManager.Application.DTOs;

namespace TelegramBotManager.Application.FinancialControl.FinanceControlMessageReceived;

public class FinanceControlMessageReceivedCommand : IRequest<Unit>
{
    public TelegramUpdateDto Request;
}
