using MediatR;
using TelegramBotManager.Application.DTOs;

namespace TelegramBotManager.Application.FinancialControl.FinanceControlMessageReceived;

public class FinanceControlMessageReceivedCommand : IRequest<FinanceControlMessageReceivedResult>
{
    public TelegramUpdateDto Request;
}
