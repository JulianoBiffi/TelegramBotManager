using MediatR;

namespace TelegramBotManager.Application.Features.FinanceControlDefineCategory;

public class FinanceControlDefineCategoryCommand : IRequest<Unit>
{
    public FinanceControlDefineCategoryCommand(int transactionId, long? categoryId = null)
    {
        TransactionId = transactionId;
        CategoryId = categoryId;
    }

    public int TransactionId { get; set; }

    public long? CategoryId { get; set; }
}
