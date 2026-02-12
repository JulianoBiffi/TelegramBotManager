using MediatR;

namespace TelegramBotManager.Application.Features.FinanceControlDeleteTransaction;

public class FinanceControlDeleteTransactionCommand : IRequest<Unit>
{
    public long TransactionId { get; set; }
}
