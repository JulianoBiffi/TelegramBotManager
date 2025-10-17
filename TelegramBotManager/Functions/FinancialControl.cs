using AutoMapper;
using Azure.Core;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types;
using TelegramBotManager.Application.DTOs;
using TelegramBotManager.Application.FinancialControl.FinanceControlMessageReceived;
using TelegramBotManager.Common.Helpers;

namespace TelegramBotManager.Functions;

/// <summary>
/// Manages financial control operations with telegram bot.
/// </summary>
public class FinancialControl(IMediator _mediator, ILogger<FinancialControl> _logger, [FromKeyedServices("FinancialQueueClient")] QueueClient _financialQueueClient)
{
    [Function("FinancialControlQueue")]
    public async Task Run(
        [QueueTrigger("financial-control-queue", Connection = "Azure:StorageConnectionString")]
        QueueMessage message,
        CancellationToken cancellationToken)
    {
        string messageBody =
            message.Body.ToString() ?? string.Empty;

        _logger.LogInformation($"New message received! {messageBody}");

        if (string.IsNullOrEmpty(messageBody))
            _logger.LogInformation($"The message is empty!");

        var update =
            JsonConvert.DeserializeObject<TelegramUpdateDto>(messageBody);

        await _financialQueueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, cancellationToken);

        await _mediator.Send(
            new FinanceControlMessageReceivedCommand() { Request = update },
            cancellationToken);
    }

    private async Task ProcessUpdateAsync(string requestBody, CancellationToken cancellationToken = default)
    {
        var update = JsonConvert.DeserializeObject<TelegramUpdateDto>(requestBody);
        //requestBody = @"{""update_id"":342538516,""message"":{""message_id"":7,""from"":{""id"":7658500418,""is_bot"":false,""first_name"":""Juliano"",""last_name"":""Biffi"",""language_code"":""pt-br""},""chat"":{""id"":7658500418,""first_name"":""Juliano"",""last_name"":""Biffi"",""type"":""private""},""date"":1759431417,""text"":""oi""}}";


        try
        {
            var jsonObj = JObject.Parse(requestBody);
            jsonObj["message"]?["date"]?.Parent?.Remove();
            var cleanedJson = jsonObj.ToString();
            var updat23e = JsonConvert.DeserializeObject<Update>(cleanedJson);
        }
        catch (Exception)
        {
        }

        //var response = await _mediator.Send(new FinanceControlMessageReceivedCommand(), cancellationToken);
    }
}