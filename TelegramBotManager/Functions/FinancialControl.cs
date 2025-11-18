using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TelegramBotManager.Application.DTOs;
using TelegramBotManager.Application.FinancialControl.FinanceControlMessageReceived;
using TelegramBotManager.Common.Exceptions;

namespace TelegramBotManager.Functions;

/// <summary>
/// Manages financial control operations with telegram bot.
/// </summary>
public class FinancialControl(IMediator _mediator, ILogger<FinancialControl> _logger, [FromKeyedServices("FinancialQueueClient")] QueueClient _financialQueueClient)
{
    [Function("FinancialControlQueue")]
    public async Task Run(
        [QueueTrigger("financial-control-queue", Connection = "Azure_StorageConnectionString")]
        QueueMessage message,
        CancellationToken cancellationToken)
    {
        string messageBody =
            message.Body.ToString() ?? string.Empty;

        _logger.LogInformation($"New message received! {messageBody}");

        if (string.IsNullOrEmpty(messageBody))
            _logger.LogInformation($"The message is empty!");

        await _financialQueueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, cancellationToken);

        try
        {
            var update =
                JsonConvert.DeserializeObject<TelegramUpdateDto>(messageBody, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

            await _mediator.Send(
                new FinanceControlMessageReceivedCommand() { Request = update },
                cancellationToken);
        }
        catch (TelegramException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new TelegramException(ex.Message, ex);
        }
    }
}