using Azure;
using Supabase;
using System.Threading;
using TelegramBotManager.Common.Helpers;
using TelegramBotManager.Domain.Entities.FinancialControl;
using TelegramBotManager.Domain.Interfaces;
using TelegramBotManager.Infrastructure.Persistence.Models;
using TelegramBotManager.Infrastructure.Repositories.Mappers;
using static Supabase.Postgrest.Constants;

namespace TelegramBotManager.Infrastructure.Repositories;

public class TransactionRepository : BaseRepository<TransactionModel>, ITransactionRepository
{
    public TransactionRepository(Client supabaseClient) : base(supabaseClient)
    {
    }

    public async Task<List<Transaction>> GetTransactions(CancellationToken cancellationToken)
    {
        var models = await GetAllAsync(cancellationToken);
        return models.Select(TransactionMapper.ToDomain).ToList();
    }

    public async Task<Transaction> SaveAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        var model = TransactionMapper.ToModel(transaction);

        var result = model.Id > 0
            ? await base.UpdateAsync(model, cancellationToken)
            : await base.InsertAsync(model, cancellationToken);

        return TransactionMapper.ToDomain(result);
    }

    public async Task<decimal> GetAmmountOfMonth(
        Transaction transaction,
        CancellationToken cancellationToken,
        bool filterByCategory = false)
    {
        var startDate = DateTimeHelper.GetFirstDayOfThisMonth().ToString("yyyy-MM-dd");
        var endDate = DateTimeHelper.GetLastDayOfThisMonth().ToString("yyyy-MM-dd");

        var query =
            _supabaseClient
            .From<TransactionModel>()
            .Select("value")
            .Filter("date", Operator.GreaterThanOrEqual, startDate)
            .Filter("date", Operator.LessThanOrEqual, endDate);

        if (filterByCategory)
        {
            if (transaction.CategoryId.HasValue)
            {
                var categoryId = transaction.CategoryId.ToString();

                query = query.Filter("category_id", Operator.Equals, categoryId);
            }
            else
                query = query.Filter<TransactionModel>("category_id", Operator.Is, null);
        }

        var response =
            await query.Get();

        if (response.Models == null || !response.Models.Any())
            return 0;

        return response.Models.Sum(t => t.Value);
    }

    public async Task<List<Transaction>> GetTransactionsByPeriod(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var query =
            _supabaseClient
            .From<TransactionModel>()
            .Select("*")
            .Filter("date", Operator.GreaterThanOrEqual, startDate.ToString("o"))
            .Filter("date", Operator.LessThanOrEqual, endDate.ToString("o"));

        var response =
            await query.Get();

        if (response.Models == null || !response.Models.Any())
            return new List<Transaction>();

        return response.Models.Select(TransactionMapper.ToDomain).ToList();
    }

    public async Task<List<Transaction>> GetTransactionsByPeriod(DateTime referenceDate, CancellationToken cancellationToken)
    {
        var result = await _supabaseClient.Rpc<List<TransactionModel>>(
            "get_transactions_by_closing_period",
            new Dictionary<string, object>
            {
                { "reference_date", referenceDate }
            });

        if (result == null || !result.Any())
            return new List<Transaction>();

        return result.Select(TransactionMapper.ToDomain).ToList();
    }

    public async Task<Transaction> GetTransactionById(long transactionId, CancellationToken cancellationToken)
    {
        var model = await GetByIdAsync(transactionId, cancellationToken);
        return TransactionMapper.ToDomain(model);
    }

    public async Task<bool> TransactionExists(DateTime date, decimal value, string description, CancellationToken cancellationToken)
    {
        // Define uma janela de tempo para considerar duplicidade (ex: mesmo segundo exato)
        // Como o parser pode retornar a data exata do timestamp, usamos ela.
        // Convertendo para formato compatível com o filtro (ISO 8601 string)
        
        var dateStr = date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        
        // Supabase/Postgrest filter syntax
        var query = _supabaseClient
            .From<TransactionModel>()
            .Select("id")
            .Filter("value", Operator.Equals, value.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .Filter("date", Operator.Equals, dateStr);
            
        // Adiciona filtro por descrição (like/ilike pode ser mais flexível, mas equals é mais seguro para evitar falsos positivos se a descrição vier exata)
        // Se a descrição for longa ou variar, pode ser melhor usar ILike ou verificar apenas parte dela.
        // Dado que vem de notificação automática, assumimos que o texto será consistente.
        query = query.Filter("description", Operator.Equals, description);

        var result = await query.Get();

        return result.Models != null && result.Models.Any();
    }
}
