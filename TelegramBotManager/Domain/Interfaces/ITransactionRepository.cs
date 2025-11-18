using TelegramBotManager.Domain.Entities.FinancialControl;

namespace TelegramBotManager.Domain.Interfaces;

public interface ITransactionRepository
{
    Task<transaction> GetTransactionById(int transactionId, CancellationToken cancellationToken);
    Task<List<transaction>> GetTransactions(CancellationToken cancellationToken);
    Task<transaction> SaveAsync(transaction transaction, CancellationToken cancellationToken);
    Task<decimal> GetAmmountOfMonth(transaction transaction, CancellationToken cancellationToken, bool filterByCategory = false);
    Task<List<transaction>> GetTransactionsByPeriod(DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
}