using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using TelegramBotManager.Application.DTOs;
using TelegramBotManager.Application.Interfaces;
using TelegramBotManager.Common.Exceptions;
using TelegramBotManager.Common.Helpers;
using TelegramBotManager.Configurations;

namespace TelegramBotManager.Functions;

/// <summary>
/// Manages financial control operations with telegram bot.
/// </summary>
public class FinancialControl(
    IMediator _mediator,
    ILogger<FinancialControl> _logger,
    [FromKeyedServices("FinancialQueueClient")] QueueClient _financialQueueClient,
    ITelegramMessageRouter _router,
    [FromKeyedServices("FinancialControl")] TelegramBotClient _telegramBotClient,
    FinancialControlOptions _financialControlOptions)
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

            await _telegramBotClient.SendTyping(_financialControlOptions.AllowedGroup.ToString());

            var command = _router.RouteMessage(update);

            if (command != null)
            {
                await _mediator.Send(command, cancellationToken);
            }
            else
            {
                await _telegramBotClient.PrintListOfOptions(_financialControlOptions.AllowedGroup.ToString());
            }
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