using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotManager.Application.DTOs;
using TelegramBotManager.Common.Helpers;
using TelegramBotManager.Configurations;

namespace TelegramBotManager.Application.FinancialControl.FinanceControlMessageReceived;

public class FinanceControlMessageReceivedHandler(
    IMapper _mapper,
    FinancialControlOptions _financialControlOptions,
    [FromKeyedServices("FinancialControl")] TelegramBotClient _telegramBotClient) : IRequestHandler<FinanceControlMessageReceivedCommand, FinanceControlMessageReceivedResult>
{
    public async Task<FinanceControlMessageReceivedResult> Handle(FinanceControlMessageReceivedCommand command, CancellationToken cancellationToken)
    {
        var validator =
            new FinanceControlMessageReceivedValidator();

        var validationResult =
            await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        /*
         comandos:
        /excluir ID
        /relatorio mês ou range
        /cadastro

          
         */

        string receivedText =
            command.Request.Message.Text.ToLower();

        switch (receivedText)
        {
            case var text when text.Contains("/cadastro"):
                await ProcessCreateRecords(receivedText, cancellationToken);
                break;
            default:
                await _telegramBotClient.PrintListOfOptions(_financialControlOptions.AllowedGroup.ToString());
                break;
        }


        return new FinanceControlMessageReceivedResult();
    }

    private async Task ProcessCreateRecords(string receivedText, CancellationToken cancellationToken)
    {
        var currentDto = FinancialControlCreateRecordParser.ToDto(receivedText);
    }
}
