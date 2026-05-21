using System.Text;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotManager.Configurations;
using TelegramBotManager.Domain.Interfaces;

namespace TelegramBotManager.Application.Features.CreditCardListClosingDate;

public class CreditCardListClosingDateHandler(
    ICreditCardClosingDateRepository _repository,
    [FromKeyedServices("FinancialControl")] TelegramBotClient _telegramBotClient,
    FinancialControlOptions _financialControlOptions,
    ILogger<CreditCardListClosingDateHandler> _logger) : IRequestHandler<CreditCardListClosingDateCommand, Unit>
{
    public async Task<Unit> Handle(
        CreditCardListClosingDateCommand request,
        CancellationToken cancellationToken)
    {
        var listOfCreditCardClosingDates =
            await _repository.GetAllAsync(cancellationToken);

        if (!listOfCreditCardClosingDates.Any())
        {
            await _telegramBotClient.SendMessage(
                _financialControlOptions.AllowedGroup,
                "Nenhuma data de fechamento de cartão encontrada!",
                parseMode: ParseMode.Markdown);
        }
        var message =
            new StringBuilder();

        message.AppendLine();

        var allButtons = new List<InlineKeyboardButton[]>();
        listOfCreditCardClosingDates.ForEach(creditCard =>
        {
            allButtons.Add(
                 new[]
                 {
                     InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(
                        creditCard.BankName,
                        $"\n/datafechamentocartoes&bankName={creditCard.BankName}&closingDate={creditCard.ClosingDate}&bestDayToBuy={creditCard.BestDayToBuy}\n"
                        ),
                });
        });

        allButtons.Add(
            new[]
            {
                InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(
                    "    Cadastro de novo fechamento de cartão    ",
                    $"\n/datafechamentocartoes&bankName=NOME_BANCO&closingDate=DATA_FECHAMENTO&bestDayToBuy=MELHOR_DIA_DE_COMPRA\n"
                    )
            });

        var inlineKeyboard =
           new InlineKeyboardMarkup(allButtons.ToArray());

        await _telegramBotClient.SendMessage(
            _financialControlOptions.AllowedGroup,
            "🔄 *Lista de datas de fechamento dos cartões (Clique para alterar)*",
            replyMarkup: inlineKeyboard);

        return Unit.Value;
    }
}
