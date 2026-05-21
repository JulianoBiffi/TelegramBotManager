using MediatR;

namespace TelegramBotManager.Application.FinancialControl.FinanceControlCreateTransaction;


public class FinanceControlCreateTransactionCommand : IRequest<Unit>
{
    public string MessageBody { get; set; }
}
