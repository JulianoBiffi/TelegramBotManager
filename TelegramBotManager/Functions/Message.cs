using Azure.Storage.Queues;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TelegramBotManager.Application.DTOs;
using TelegramBotManager.Application.Features.BankTransactionAutoSave;
using TelegramBotManager.Common.Helpers;
using TelegramBotManager.Configurations;

namespace TelegramBotManager.Functions;

/// <summary>
/// Manages financial control operations with telegram bot.
/// </summary>
public class Message(
    ILogger<TelegramMessage> _logger,
    FinancialControlOptions _financialControlOptions,
    IMediator _mediator,
    [FromKeyedServices("FinancialMessageQueueClient")] QueueClient _financialQueueClient)
{
    /// <summary>
    /// Handles incoming Telegram messages and enqueue to the right Azure Storage Queue.
    /// Also processes bank transaction messages automatically.
    /// </summary>
    /// <remarks>The telegram has a weird behavior to resend a message if not receive a response as more soon possible (need to be less than 1 seconds).
    /// So the usage of Azure Storage Queue is to solve that problem.</remarks>
    /// <param name="req">The HTTP request containing the message to process.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.  Returns an <see cref="OkResult"/>
    /// regardless of the processing outcome.</returns>
    [Function("Message")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"New message received");

        string requestBody =
            await req.GetBodyAsStringAsync(cancellationToken);

        if (string.IsNullOrEmpty(requestBody))
            return new BadRequestObjectResult("Request body is empty");

        PurchaseDto currentPurchaseDto = null;
        try
        {
            currentPurchaseDto =
                JsonConvert.DeserializeObject<PurchaseDto>(
                    requestBody,
                    new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

            if (currentPurchaseDto == null || string.IsNullOrEmpty(currentPurchaseDto.Package) || string.IsNullOrEmpty(currentPurchaseDto.FullText))
                throw new Exception("Request body is not a valid PurchaseDto");

        }
        catch (Exception ex)
        {
            var message =
                await _financialQueueClient.SendMessageAsync(requestBody, cancellationToken);

            return new BadRequestObjectResult($"Erro ao converter o objeto: {ex.Message}");
        }

        try
        {
            var autoSaveCommand =
                new BankTransactionAutoSaveCommand(currentPurchaseDto);

            var autoSaveResult =
                await _mediator.Send(autoSaveCommand, cancellationToken);

            if (autoSaveResult.Success)
            {
                string logOfStatus =
                    autoSaveResult.IsDuplicate
                    ? $"Transação bancária já existe na base!"
                    : $"Transação bancária processada automaticamente: {autoSaveResult.Message}";
                _logger.LogInformation(logOfStatus);

                return new OkObjectResult(logOfStatus);
            }

            var message =
                await _financialQueueClient.SendMessageAsync(requestBody, cancellationToken);

            return new BadRequestObjectResult(autoSaveResult.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao tentar processar como transação bancária, enfileirando para processamento normal");

            var message =
                await _financialQueueClient.SendMessageAsync(requestBody, cancellationToken);

            return new BadRequestObjectResult(ex.Message);
        }
    }
}