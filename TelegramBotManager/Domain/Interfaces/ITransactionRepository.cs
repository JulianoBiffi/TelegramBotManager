using TelegramBotManager.Domain.Entities.FinancialControl;

namespace TelegramBotManager.Domain.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction> GetTransactionById(long transactionId, CancellationToken cancellationToken);
    Task<List<Transaction>> GetTransactions(CancellationToken cancellationToken);
    Task<Transaction> SaveAsync(Transaction transaction, CancellationToken cancellationToken);
    Task<decimal> GetAmmountOfMonth(Transaction transaction, CancellationToken cancellationToken, bool filterByCategory = false);
    Task<List<Transaction>> GetTransactionsByPeriod(DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
    Task<bool> TransactionExists(DateTime date, decimal value, string description, CancellationToken cancellationToken);
}