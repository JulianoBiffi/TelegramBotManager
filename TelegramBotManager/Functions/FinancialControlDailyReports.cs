using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TelegramBotManager.Application.Features.FinancialControlDailyReports;

namespace TelegramBotManager.Functions;

public class FinancialControlDailyReports(IMediator _mediator, ILogger<FinancialControlDailyReports> _logger)
{
    [Function("FinancialControlDailyReports")]
    public async Task Run([TimerTrigger("0 8 * * *")] TimerInfo myTimer, CancellationToken cancellationToken)
    {
        //TODO: Remove
        // PRD: "0 8 * * *"
        // Local: "*/1 * * * *"
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }

        await _mediator.Send(
            new FinancialControlDailyReportsCommand(),
            cancellationToken);
    }
}