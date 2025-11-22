using Azure;
using Supabase;
using System.Threading;
using TelegramBotManager.Common.Helpers;
using TelegramBotManager.Domain.Entities.FinancialControl;
using TelegramBotManager.Domain.Interfaces;
using static Supabase.Postgrest.Constants;

namespace TelegramBotManager.Infrastructure.Repositories;

public class TransactionRepository : BaseRepository<transaction>, ITransactionRepository
{
    public TransactionRepository(Client supabaseClient) : base(supabaseClient)
    {
    }

    public async Task<List<transaction>> GetTransactions(CancellationToken cancellationToken)
        => await GetAllAsync(cancellationToken);

    public async Task<transaction> SaveAsync(transaction transaction, CancellationToken cancellationToken)
    {
        return transaction.Id > 0
            ? await base.UpdateAsync(transaction, cancellationToken)
            : await base.InsertAsync(transaction, cancellationToken);
    }

    public async Task<transaction> SaveAsync(string transactionDescription, CancellationToken cancellationToken)
    {
        return
            (await _supabaseClient.From<transaction>()
                                 .Select("*")
                                 .Filter(nameof(transaction.Description), Operator.Like, transactionDescription)
                                 .Get(cancellationToken: cancellationToken))
                                 .Model;
    }

    public async Task<decimal> GetAmmountOfMonth(
        transaction transaction,
        CancellationToken cancellationToken,
        bool filterByCategory = false)
    {
        var startDate = DateTimeHelper.GetFirstDayOfThisMonth().ToString("yyyy-MM-dd");
        var endDate = DateTimeHelper.GetLastDayOfThisMonth().ToString("yyyy-MM-dd");

        var query =
            _supabaseClient
            .From<transaction>()
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
                query = query.Filter<transaction>("category_id", Operator.Is, null);
        }

        var response =
            await query.Get();

        if (response.Models == null || !response.Models.Any())
            return 0;

        return response.Models.Sum(t => t.Value);
    }

    public async Task<List<transaction>> GetTransactionsByPeriod(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var query =
            _supabaseClient
            .From<transaction>()
            .Select("*")
            .Filter("date", Operator.GreaterThanOrEqual, startDate.ToString("o"))
            .Filter("date", Operator.LessThanOrEqual, endDate.ToString("o"));

        var response =
            await query.Get();

        if (response.Models == null || !response.Models.Any())
            return new List<transaction>();

        return response.Models;
    }

    public async Task<transaction> GetTransactionById(long transactionId, CancellationToken cancellationToken)
        => await GetByIdAsync(transactionId, cancellationToken);
}