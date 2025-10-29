using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TelegramBotManager.Application.FinancialControl.FinanceControlCreateTransaction;
using TelegramBotManager.Common.Exceptions;
using TelegramBotManager.Common.Helpers;
using TelegramBotManager.Configurations;

namespace TelegramBotManager.Application.FinancialControl.FinanceControlMessageReceived;

public class FinanceControlMessageReceivedHandler(
    IMediator _mediator,
    FinancialControlOptions _financialControlOptions,
    [FromKeyedServices("FinancialControl")] TelegramBotClient _telegramBotClient) : IRequestHandler<FinanceControlMessageReceivedCommand, Unit>
{
    public async Task<Unit> Handle(FinanceControlMessageReceivedCommand command, CancellationToken cancellationToken)
    {
        var validator =
            new FinanceControlMessageReceivedValidator();

        var validationResult =
            await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (!_financialControlOptions.AllowedUserIds.Any(user => command.Request.Message.From.Id == user))
            throw new TelegramException($"O usuário {command.Request.Message.From.Id} não está autorizado a enviar mensagens!");

        string receivedText =
            command.Request.Message.Text.ToLower();

        switch (receivedText)
        {
            case var text when text.Contains("/cadastro"):
                var createTransactionCommand =
                    new FinanceControlCreateTransactionCommand()
                    {
                        MessageBody = receivedText
                    };

                var result =
                    await _mediator.Send(createTransactionCommand, cancellationToken);

                if (result == null)
                    throw new TelegramException("Erro ao cadastrar a transação!");

                if (result.Category != null)
                    await _telegramBotClient.PrintCreatedTransaction(_financialControlOptions.AllowedGroup, result);

                break;
            case var text when text.Contains("/relatorio"):
                break;
            case var text when text.Contains("/excluir"):
                break;
            default:
                await _telegramBotClient.PrintListOfOptions(_financialControlOptions.AllowedGroup.ToString());
                break;
        }


        return Unit.Value;
    }
}
