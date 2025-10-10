using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using TelegramBotManager.Common;
using TelegramBotManager.Common.Validations;

namespace TelegramBotManager.Middleware;

public class ValidationExceptionMiddleware(ILogger<ValidationExceptionMiddleware> _logger) : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation error occurred.");
            await HandleValidationExceptionAsync(context, ex);
        }
    }

    private static async Task HandleValidationExceptionAsync(FunctionContext context, ValidationException exception)
    {
        var httpReqData = await context.GetHttpRequestDataAsync();

        if (httpReqData == null)
            return;

        var response = httpReqData.CreateResponse(HttpStatusCode.BadRequest);
        response.Headers.Add("Content-Type", "application/json");

        var apiResponse =
            new ApiResponse
            {
                Success = false,
                Message = "Error in validations",
                Errors = exception.Errors.Select(error => (ValidationErrorDetail)error)
            };

        await response.WriteStringAsync(
            JsonSerializer.Serialize(
                apiResponse,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

        context.GetInvocationResult().Value = response;

    }
}