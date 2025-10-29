using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramBotManager.Common;
using TelegramBotManager.Common.Exceptions;
using TelegramBotManager.Common.Helpers;
using TelegramBotManager.Configurations;

namespace TelegramBotManager.Middleware;

public class TelegramExceptionMiddleware(
    ILogger<TelegramExceptionMiddleware> _logger,
    FinancialControlOptions _financialControlOptions,
    [FromKeyedServices("FinancialControl")] TelegramBotClient _telegramBotClient) : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (TelegramException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
    }

    private async Task HandleValidationExceptionAsync(FunctionContext context, TelegramException exception)
    {
        _logger.LogError(exception, "Error while attempt to process telegram message");

        if (exception.TelegramBotType == Common.Enum.TelegramBotTypeEnum.FinancialControl)
            await HandleFinancialControlMessages(exception);

        var httpReqData = await context.GetHttpRequestDataAsync();

        if (httpReqData == null)
            return;

        var response = httpReqData.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");

        context.GetInvocationResult().Value = response;

    }

    private async Task HandleFinancialControlMessages(TelegramException exception)
    {
        if (exception.TelegramInvalidOption.HasValue)
        {
            switch (exception.TelegramInvalidOption.Value)
            {
                case Common.Enum.TelegramInvalidOptionExceptionEnum.InvalidTransactionFormat:
                    await _telegramBotClient.PrintCreateTransaction(_financialControlOptions.AllowedGroup, exception.TelegramMessage);
                    break;
                default:
                    await _telegramBotClient.PrintListOfOptions(exception.TelegramMessage);
                    break;
            }

            return;
        }

        await _telegramBotClient.SendMessage(
            _financialControlOptions.AllowedGroup,
            $"Erro ao tentar processar a mensagem: {exception.TelegramMessage}");
    }
}
