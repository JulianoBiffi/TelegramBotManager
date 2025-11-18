using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TelegramBotManager.Application.Features.FinanceControlDefineCategory;
using TelegramBotManager.Application.Features.FinancialControlDailyReports;
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
        await _telegramBotClient.SendTyping(_financialControlOptions.AllowedGroup.ToString());

        var validator =
            new FinanceControlMessageReceivedValidator();

        var validationResult =
            await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (!_financialControlOptions.AllowedUserIds.Any(user => command.Request.Message?.From?.Id == user) &&
                !_financialControlOptions.AllowedUserIds.Any(user => command.Request.CallbackQuery?.From?.Id == user))
            throw new TelegramException($"O usuário {command.Request.Message?.From?.Id ?? command.Request.CallbackQuery?.From?.Id} não está autorizado a enviar mensagens!");

        if (string.IsNullOrEmpty(command.Request.Message?.Text ?? string.Empty) && string.IsNullOrEmpty(command.Request.CallbackQuery?.Data))
            throw new TelegramException($"Erro ao processar a mensagem!");

        string selectedOption =
            command.Request.Message?.Text?.ToLower() ?? string.Empty;

        selectedOption +=
            command.Request.CallbackQuery?.Data?.ToLower() ?? string.Empty;

        switch (selectedOption)
        {
            case var text when text.Contains("/cadastro"):
                var createTransactionCommand =
                    new FinanceControlCreateTransactionCommand()
                    {
                        MessageBody = command.Request.Message.Text.ToLower()
                    };

                var result =
                    await _mediator.Send(createTransactionCommand, cancellationToken);

                if (result == null)
                    throw new TelegramException("Erro ao cadastrar a transação!");

                await _telegramBotClient.PrintCreatedTransaction(_financialControlOptions.AllowedGroup, result);

                if (result.Category == null)
                {
                    await _mediator.Send(new FinanceControlDefineCategoryCommand(result.Id), cancellationToken);
                }

                break;
            case var text when text.Contains("/relatoriomensal"):
                await _mediator.Send(
                    new FinancialControlDailyReportsCommand(),
                    cancellationToken);
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
