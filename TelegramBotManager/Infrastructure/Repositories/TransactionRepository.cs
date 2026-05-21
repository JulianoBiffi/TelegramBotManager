using Supabase;
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

    public async Task<decimal> GetAmountByPeriodAsync(
        DateTime startDate,
        DateTime endDate,
        long? categoryId,
        CancellationToken cancellationToken,
        bool filterByCategory = false)
    {
        var startDateStr = startDate.ToString("yyyy-MM-dd");
        var endDateStr = endDate.ToString("yyyy-MM-dd");

        var query =
            _supabaseClient
            .From<TransactionModel>()
            .Select("value")
            .Filter("date", Operator.GreaterThanOrEqual, startDateStr)
            .Filter("date", Operator.LessThanOrEqual, endDateStr);

        if (filterByCategory)
        {
            if (categoryId.HasValue)
            {
                var catIdStr = categoryId.ToString();
                query = query.Filter("category_id", Operator.Equals, catIdStr);
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

        var dateStr = date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        var query = _supabaseClient
            .From<TransactionModel>()
            .Select("id")
            .Filter("value", Operator.Equals, value.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .Filter("date", Operator.Equals, dateStr);

        query = query.Filter("description", Operator.Equals, description);

        var result = await query.Get();

        return result.Models != null && result.Models.Any();
    }
}
