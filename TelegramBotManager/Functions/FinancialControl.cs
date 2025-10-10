using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types;
using TelegramBotManager.Application.DTOs;
using TelegramBotManager.Common.Helpers;

namespace TelegramBotManager.Functions;

/// <summary>
/// Manages financial control operations with telegram bot.
/// </summary>
public class FinancialControl(ILogger<FinancialControl> _logger, [FromKeyedServices("FinancialQueueClient")] QueueClient _financialQueueClient)
{
    [Function("ProcessQueueMessage")]
    public async Task Run(
        [QueueTrigger("financial-control-queue", Connection = "Azure:StorageConnectionString")]
        QueueMessage message)
    {
        _logger.LogInformation($"New message received");
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