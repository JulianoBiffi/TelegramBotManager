using MediatR;
using TelegramBotManager.Application.DTOs;
using TelegramBotManager.Application.Features.CreditCardClosingDateManagement;
using TelegramBotManager.Application.Features.CreditCardListClosingDate;
using TelegramBotManager.Application.Features.FinanceControlCreateCategory;
using TelegramBotManager.Application.Features.FinanceControlDefineCategory;
using TelegramBotManager.Application.Features.FinanceControlDeleteTransaction;
using TelegramBotManager.Application.Features.FinanceControlEditTransactionsOfMonth;
using TelegramBotManager.Application.Features.FinanceControlListTransactionsToDelete;
using TelegramBotManager.Application.Features.FinancialControlDailyReports;
using TelegramBotManager.Application.FinancialControl.FinanceControlCreateTransaction;
using TelegramBotManager.Application.Interfaces;
using TelegramBotManager.Common.Exceptions;
using TelegramBotManager.Common.Helpers;
using TelegramBotManager.Configurations;

namespace TelegramBotManager.Application.Services;

public class TelegramMessageRouter : ITelegramMessageRouter
{
    private readonly FinancialControlOptions _financialControlOptions;

    public TelegramMessageRouter(FinancialControlOptions financialControlOptions)
    {
        _financialControlOptions = financialControlOptions;
    }

    public IRequest<Unit> RouteMessage(TelegramUpdateDto update)
    {
        if (!_financialControlOptions.AllowedUserIds.Any(user => update.Message?.From?.Id == user) &&
            !_financialControlOptions.AllowedUserIds.Any(user => update.CallbackQuery?.From?.Id == user))
        {
            throw new TelegramException($"O usuário {update.Message?.From?.Id ?? update.CallbackQuery?.From?.Id} não está autorizado a enviar mensagens!");
        }

        if (string.IsNullOrEmpty(update.Message?.Text ?? string.Empty) && string.IsNullOrEmpty(update.CallbackQuery?.Data))
        {
            throw new TelegramException("Erro ao processar a mensagem! Texto ou Data vazios.");
        }

        string selectedOption = (update.Message?.Text?.ToLower() ?? string.Empty)
                              + (update.CallbackQuery?.Data?.ToLower() ?? string.Empty);

        switch (selectedOption)
        {
            case var text when text.Contains("/cadastro"):
                return new FinanceControlCreateTransactionCommand()
                {
                    MessageBody = update.Message?.Text?.ToLower() ?? update.CallbackQuery?.Data?.ToLower()
                };

            case var text when text.Contains("/relatoriomensal"):
                return new FinancialControlDailyReportsCommand();

            case var text when text.Contains("/editarlancamentosdomes"):
                return new FinanceControlEditTransactionsOfMonthCommand();

            case var text when text.Contains("/listafechamentocartoes"):
                return new CreditCardListClosingDateCommand();

            case var text when text.Contains("/datafechamentocartoes"):
                return new CreditCardClosingDateManagementCommand()
                {
                    MessageBody = selectedOption
                };

            case var text when text.Contains("/relatorio"):
                return null; // O handler original fazia um break sem retornar nada

            case var text when text.Contains("/definircategoria"):
                var transactionId = text.TryTakeValueFromString("transactionid");
                var categoryId = text.TryTakeValueFromString("categoryid");

                if (!transactionId.HasValue)
                    throw new TelegramException($"Id da transação não informada! \n{text}");

                if (!categoryId.HasValue)
                    return new FinanceControlDefineCategoryCommand(transactionId.Value);
                else
                    return new FinanceControlDefineCategoryCommand(transactionId.Value, categoryId.Value);

            case var text when text.Contains("/cadastrarcategoria"):
                return new FinanceControlCreateCategoryCommand()
                {
                    MessageBody = selectedOption
                };

            case var text when text.Contains("/excluirlancamentos"):
                return new FinanceControlListTransactionsToDeleteCommand();

            case var text when text.Contains("/deletartransacao"):
                var transactionIdToDelete = text.TryTakeValueFromString("transactionid");

                if (!transactionIdToDelete.HasValue)
                    throw new TelegramException($"Id da transação não informada! \n{text}");

                return new FinanceControlDeleteTransactionCommand() { TransactionId = transactionIdToDelete.Value };

            default:
                return null; // A Azure Function vai lidar com o default de enviar o menu de opções
        }
    }
}
