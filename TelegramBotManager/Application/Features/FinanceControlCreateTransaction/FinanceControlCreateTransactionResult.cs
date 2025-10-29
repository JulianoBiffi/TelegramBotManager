using Newtonsoft.Json;
using TelegramBotManager.Domain.Entities.FinancialControl;

namespace TelegramBotManager.Application.FinancialControl.FinanceControlCreateTransaction;

public class FinanceControlCreateTransactionResult
{
    public long Id { get; set; }

    public DateTime Date { get; set; }

    public string CreditCard { get; set; }

    public decimal Value { get; set; }

    public string Description { get; set; }

    public category Category { get; set; }

    public decimal AmmountOfMonth { get; set; }

    public decimal AmmountOfThisCategory { get; set; }
}
