using MediatR;

namespace TelegramBotManager.Application.Features.FinanceControlDefineCategory;

public class FinanceControlDefineCategoryCommand : IRequest<Unit>
{
    public FinanceControlDefineCategoryCommand(long transactionId, long? categoryId = null)
    {
        TransactionId = transactionId;
        CategoryId = categoryId;
    }

    public long TransactionId { get; set; }

    public long? CategoryId { get; set; }
}
