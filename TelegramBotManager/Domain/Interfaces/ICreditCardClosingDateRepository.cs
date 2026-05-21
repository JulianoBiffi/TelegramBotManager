using TelegramBotManager.Domain.Entities.FinancialControl;

namespace TelegramBotManager.Domain.Interfaces;

public interface ICreditCardClosingDateRepository
{
    Task<CreditCardClosingDate?> GetByBankNameAsync(string bankName, CancellationToken cancellationToken);
    Task<CreditCardClosingDate> SaveAsync(CreditCardClosingDate entity, CancellationToken cancellationToken);
    Task<List<CreditCardClosingDate>> GetAllAsync(CancellationToken cancellationToken);
}
