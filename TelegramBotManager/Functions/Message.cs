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
        {
            _logger.LogError("Request body is empty");
            return new OkResult();
        }

        try
        {
            // Deserializar para verificar se é uma mensagem de texto
            var update = JsonConvert.DeserializeObject<TelegramUpdateDto>(requestBody, 
                new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

            var messageText = update?.Message?.Text;

            // Se houver texto na mensagem, tentar processar como transação bancária
            if (!string.IsNullOrEmpty(messageText))
            {
                _logger.LogInformation("Tentando processar como transação bancária...");

                var autoSaveCommand = new BankTransactionAutoSaveCommand
                {
                    MessageBody = messageText
                };

                var autoSaveResult = await _mediator.Send(autoSaveCommand, cancellationToken);

                if (autoSaveResult.Success)
                {
                    _logger.LogInformation($"Transação bancária processada automaticamente: {autoSaveResult.Message}");
                    
                    // Não enfileirar se foi processada com sucesso como transação bancária
                    // A menos que precise notificar o usuário (pode ser implementado aqui)
                    return new OkResult();
                }
                else
                {
                    _logger.LogInformation("Mensagem não identificada como transação bancária, enfileirando para processamento normal");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao tentar processar como transação bancária, enfileirando para processamento normal");
        }

        // Se não foi processado como transação bancária, enfileirar normalmente
        var message =
            await _financialQueueClient.SendMessageAsync(requestBody, cancellationToken);

        _logger.LogInformation($"Message has saved with id: {message?.Value?.MessageId}");

        return new OkResult();
    }
}