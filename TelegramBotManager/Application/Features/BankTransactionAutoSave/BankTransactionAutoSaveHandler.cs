using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using TelegramBotManager.Application.Features.FinanceControlDefineCategory;
using TelegramBotManager.Application.FinancialControl.FinanceControlCreateTransaction;
using TelegramBotManager.Application.Interfaces;
using TelegramBotManager.Common.Helpers;
using TelegramBotManager.Configurations;
using TelegramBotManager.Domain.Entities.FinancialControl;
using TelegramBotManager.Domain.Interfaces;

namespace TelegramBotManager.Application.Features.BankTransactionAutoSave;

public class BankTransactionAutoSaveHandler(
    IEnumerable<IBankTransactionParser> _parsers,
    ITransactionRepository _transactionRepository,
    ICategoryRepository _categoryRepository,
    FinancialControlOptions _financialControlOptions,
    IMediator _mediator,
    [FromKeyedServices("FinancialControl")] TelegramBotClient _telegramBotClient,
    ILogger<BankTransactionAutoSaveHandler> _logger) : IRequestHandler<BankTransactionAutoSaveCommand, BankTransactionAutoSaveResult>
{
    public async Task<BankTransactionAutoSaveResult> Handle(
        BankTransactionAutoSaveCommand command,
        CancellationToken cancellationToken)
    {
        var result = new BankTransactionAutoSaveResult
        {
            Success = false
        };

        try
        {
            // Tentar encontrar um parser que consiga processar a mensagem
            IBankTransactionParser? matchedParser = null;
            foreach (var parser in _parsers)
            {
                if (parser.CanParse(command.MessageBody))
                {
                    matchedParser = parser;
                    break;
                }
            }

            if (matchedParser == null)
            {
                _logger.LogInformation("Nenhum parser encontrado para a mensagem");
                return result;
            }

            _logger.LogInformation($"Parser encontrado: {matchedParser.BankName}");

            // Fazer o parse da mensagem
            var bankTransaction = matchedParser.Parse(command.MessageBody);

            if (!bankTransaction.IsValid)
            {
                _logger.LogWarning($"Mensagem parseada mas inválida: {command.MessageBody}");
                result.ErrorMessage = "Dados da transação inválidos";
                return result;
            }

            // Verificar se a transação já existe (mesmo horário e estabelecimento)
            var exists = await _transactionRepository.TransactionExists(
                bankTransaction.Date,
                bankTransaction.Value,
                bankTransaction.Description,
                cancellationToken);

            if (exists)
            {
                _logger.LogInformation($"Transação duplicada ignorada: {bankTransaction.Description} - R$ {bankTransaction.Value}");
                result.Success = true;
                result.IsDuplicate = true;
                result.Message = "Transação já cadastrada anteriormente";
                return result;
            }

            // Tentar encontrar categoria automaticamente
            var category = await _categoryRepository.GetCategoryByTranscationDesciption(bankTransaction.Description);

            // Criar a transação (incluindo lógica de parcelamento se aplicável, por enquanto sem parcelamento no parser)
            // Futuramente os parsers podem retornar ParcelNumber se a mensagem tiver essa info
            int? parcelNumber = null; 
            
            var transactions = Transaction.CreateInstallments(
                bankTransaction.Description,
                bankTransaction.Value,
                bankTransaction.Date,
                bankTransaction.CreditCard,
                category,
                parcelNumber);

            Transaction savedTransaction = null;

            // Persist all generated transactions
            foreach (var transaction in transactions)
            {
                var saved = await _transactionRepository.SaveAsync(transaction, cancellationToken);
                if (savedTransaction == null) 
                {
                    savedTransaction = saved;
                }
            }

            _logger.LogInformation($"Transação salva automaticamente: {savedTransaction.Description} - R$ {savedTransaction.Value}");

            // Obter valores atualizados para retorno
            var ammountOfMonth =
                await _transactionRepository.GetAmmountOfMonth(savedTransaction, cancellationToken);

            var ammountOfThisCategory = category != null ?
                await _transactionRepository.GetAmmountOfMonth(savedTransaction, cancellationToken, true) : 0;

            var createResult = new FinanceControlCreateTransactionResult()
            {
                Description = savedTransaction.Description,
                CreditCard = savedTransaction.CreditCard,
                Date = savedTransaction.Date,
                Id = savedTransaction.Id,
                Value = savedTransaction.Value,
                Category = category,
                AmmountOfMonth = ammountOfMonth,
                AmmountOfThisCategory = ammountOfThisCategory
            };

            // Notificar no Telegram
            await _telegramBotClient.PrintCreatedTransaction(_financialControlOptions.AllowedGroup, createResult);

            // Se não tem categoria, solicitar definição
            if (createResult.Category == null)
            {
                await _mediator.Send(new FinanceControlDefineCategoryCommand(createResult.Id), cancellationToken);
            }

            // Montar o resultado
            result.Success = true;
            result.TransactionId = savedTransaction.Id;
            result.Description = savedTransaction.Description;
            result.Value = savedTransaction.Value;
            result.Date = savedTransaction.Date;
            result.CreditCard = savedTransaction.CreditCard;
            result.Category = category;
            result.BankSource = bankTransaction.BankSource;
            result.Message = $"Transação cadastrada automaticamente via {matchedParser.BankName}";
            result.AmmountOfMonth = ammountOfMonth;
            result.AmmountOfThisCategory = ammountOfThisCategory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar transação bancária automaticamente");
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }
}