using MediatR;
using Telegram.Bot.Types;

namespace TelegramBotManager.Application.FinancialControl.FinanceControlMessageReceived;

public class FinanceControlMessageReceivedCommand : IRequest<FinanceControlMessageReceivedResult>
{
    public Update Request;
}
