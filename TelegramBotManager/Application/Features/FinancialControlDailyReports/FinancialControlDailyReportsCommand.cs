using MediatR;

namespace TelegramBotManager.Application.Features.FinancialControlDailyReports;

public class FinancialControlDailyReportsCommand : IRequest<Unit>
{
    public long Id { get; set; }

    public long? CategoryId { get; set; }
}
