using System.Text;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramBotManager.Common.Helpers;
using TelegramBotManager.Configurations;
using TelegramBotManager.Domain.Entities.FinancialControl;
using TelegramBotManager.Domain.Interfaces;

namespace TelegramBotManager.Application.Features.CreditCardClosingDateManagement;

public class CreditCardClosingDateManagementHandler(
    ICreditCardClosingDateRepository _repository,
    [FromKeyedServices("FinancialControl")] TelegramBotClient _telegramBotClient,
    FinancialControlOptions _financialControlOptions,
    ILogger<CreditCardClosingDateManagementHandler> _logger) : IRequestHandler<CreditCardClosingDateManagementCommand, Unit>
{
    public async Task<Unit> Handle(
        CreditCardClosingDateManagementCommand request,
        CancellationToken cancellationToken)
    {
        var text = request.MessageBody.ToLower();

        var bankName = text.TryTakeStringValueFromString("bankname");
        var closingDateValue = text.TryTakeValueFromString("closingdate");
        var bestDayToBuyValue = text.TryTakeValueFromString("bestdaytobuy");

        if (string.IsNullOrEmpty(bankName))
        {
            await SendErrorMessage(
                "?? *Erro: Parâmetro obrigatório ausente*\n\n" +
                "O parâmetro `bankName` é obrigatório.\n\n" +
                "*Exemplo de uso:*\n" +
                "`/datafechamentocartoes&bankName=nu&closingDate=10&bestDayToBuy=4`",
                cancellationToken);
            return Unit.Value;
        }

        var existing = await _repository.GetByBankNameAsync(bankName, cancellationToken);

        CreditCardClosingDate entity;

        if (existing != null)
        {
            if (closingDateValue.HasValue)
            {
                if (closingDateValue.Value < 1 || closingDateValue.Value > 31)
                {
                    await SendErrorMessage(
                        "?? *Erro: Valor inválido*\n\n" +
                        "O dia de fechamento (`closingDate`) deve estar entre 1 e 31.",
                        cancellationToken);
                    return Unit.Value;
                }
                existing.UpdateClosingDate((int)closingDateValue.Value);
            }

            if (bestDayToBuyValue.HasValue)
            {
                if (bestDayToBuyValue.Value < 1 || bestDayToBuyValue.Value > 31)
                {
                    await SendErrorMessage(
                        "?? *Erro: Valor inválido*\n\n" +
                        "O melhor dia para compra (`bestDayToBuy`) deve estar entre 1 e 31.",
                        cancellationToken);
                    return Unit.Value;
                }
                existing.UpdateBestDayToBuy((int)bestDayToBuyValue.Value);
            }

            entity = await _repository.SaveAsync(existing, cancellationToken);

            await SendSuccessMessage(entity, true, cancellationToken);
        }
        else
        {
            if (!closingDateValue.HasValue || !bestDayToBuyValue.HasValue)
            {
                await SendErrorMessage(
                    "?? *Erro: Parâmetros obrigatórios ausentes*\n\n" +
                    "Para criar um novo registro, você deve fornecer tanto `closingDate` quanto `bestDayToBuy`.\n\n" +
                    "*Exemplo:*\n" +
                    "`/datafechamentocartoes&bankName=nu&closingDate=10&bestDayToBuy=4`",
                    cancellationToken);
                return Unit.Value;
            }

            if (closingDateValue.Value < 1 || closingDateValue.Value > 31)
            {
                await SendErrorMessage(
                    "?? *Erro: Valor inválido*\n\n" +
                    "O dia de fechamento (`closingDate`) deve estar entre 1 e 31.",
                    cancellationToken);
                return Unit.Value;
            }

            if (bestDayToBuyValue.Value < 1 || bestDayToBuyValue.Value > 31)
            {
                await SendErrorMessage(
                    "?? *Erro: Valor inválido*\n\n" +
                    "O melhor dia para compra (`bestDayToBuy`) deve estar entre 1 e 31.",
                    cancellationToken);
                return Unit.Value;
            }

            entity = new CreditCardClosingDate(
                bankName,
                (int)closingDateValue.Value,
                (int)bestDayToBuyValue.Value);

            entity = await _repository.SaveAsync(entity, cancellationToken);

            await SendSuccessMessage(entity, false, cancellationToken);
        }

        _logger.LogInformation(
            "Credit card closing date {Action} for bank: {BankName}",
            existing != null ? "updated" : "created",
            bankName);

        return Unit.Value;
    }

    private async Task SendSuccessMessage(
        CreditCardClosingDate entity,
        bool wasUpdate,
        CancellationToken cancellationToken)
    {
        var message = new StringBuilder();
        message.AppendLine($"? *{(wasUpdate ? "Atualizado" : "Cadastrado")} com sucesso!*");
        message.AppendLine("???????????????????????");
        message.AppendLine($"??? *Banco:* {entity.BankName}");
        message.AppendLine($"?? *Dia de Fechamento:* {entity.ClosingDate}");
        message.AppendLine($"??? *Melhor Dia para Compra:* {entity.BestDayToBuy}");

        await _telegramBotClient.SendMessage(
            _financialControlOptions.AllowedGroup,
            message.ToString(),
            parseMode: ParseMode.Markdown);
    }

    private async Task SendErrorMessage(string errorMessage, CancellationToken cancellationToken)
    {
        await _telegramBotClient.SendMessage(
            _financialControlOptions.AllowedGroup,
            errorMessage,
            parseMode: ParseMode.Markdown);
    }
}
