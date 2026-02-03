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

    public async Task<Transaction> GetTransactionById(long transactionId, CancellationToken cancellationToken)
    {
        var model = await GetByIdAsync(transactionId, cancellationToken);
        return TransactionMapper.ToDomain(model);
    }
}