using MediatR;

namespace TelegramBotManager.Application.FinancialControl.FinanceControlCreateTransaction;


public class FinanceControlCreateTransactionCommand : IRequest<FinanceControlCreateTransactionResult>
{
    public string MessageBody { get; set; }
}
