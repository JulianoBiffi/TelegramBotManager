using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
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
public class FinancialControl(ILogger<FinancialControl> _logger, QueueClient _queueClient)
{
    [Function("FinanceControlMessage")]
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

        await _queueClient.SendMessageAsync(requestBody, cancellationToken);

        return new OkResult();
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