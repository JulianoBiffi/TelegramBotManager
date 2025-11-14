using MediatR;

namespace TelegramBotManager.Application.Features.FinanceControlDefineCategory;

public class FinanceControlDefineCategoryCommand : IRequest<Unit>
{
    public long Id { get; set; }

    public long? CategoryId { get; set; }
}
