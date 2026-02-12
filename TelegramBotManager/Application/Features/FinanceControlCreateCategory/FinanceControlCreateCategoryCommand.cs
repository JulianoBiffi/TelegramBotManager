using MediatR;

namespace TelegramBotManager.Application.Features.FinanceControlCreateCategory;

public class FinanceControlCreateCategoryCommand : IRequest<Unit>
{
    public string MessageBody { get; set; }
}
