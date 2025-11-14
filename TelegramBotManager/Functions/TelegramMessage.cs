using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotManager.Common.Helpers;
using TelegramBotManager.Configurations;

namespace TelegramBotManager.Functions;

/// <summary>
/// Manages financial control operations with telegram bot.
/// </summary>
public class TelegramMessage(
    ILogger<TelegramMessage> _logger,
    FinancialControlOptions _financialControlOptions,
    [FromKeyedServices("FinancialQueueClient")] QueueClient _financialQueueClient)
{
    /// <summary>
    /// Handles incoming Telegram messages and enqueue to the right Azure Storage Queue.
    /// </summary>
    /// <remarks>The telegram has a weird behavior to resend a message if not receive a response as more soon possible (need to be less than 1 seconds).
    /// So the usage of Azure Storage Queue is to solve that problem.</remarks>
    /// <param name="req">The HTTP request containing the message to process.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.  Returns an <see cref="OkResult"/>
    /// regardless of the processing outcome.</returns>
    [Function("TelegramMessage")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"New message received");

        string requestBody =
            await req.GetBodyAsStringAsync(cancellationToken);

        if (string.IsNullOrEmpty(requestBody))
        {
            _logger.LogError("Request body is empty");
            return new OkResult();
        }

        bool isMessagFromFinancialControlGroups =
            requestBody.Contains(_financialControlOptions.AllowedGroup.ToString());

        if (!isMessagFromFinancialControlGroups)
        {
            _logger.LogError("Current message is not allowed!");
            return new OkResult();
        }

        if (isMessagFromFinancialControlGroups)
        {
            var message =
                await _financialQueueClient.SendMessageAsync(requestBody, cancellationToken);

            _logger.LogInformation($"Message has saved with id: {message?.Value?.MessageId}");
        }

        return new OkResult();
    }
}