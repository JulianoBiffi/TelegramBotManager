using TelegramBotManager.Domain.Entities.FinancialControl;

namespace TelegramBotManager.Application.Features.BankTransactionAutoSave;

public class BankTransactionAutoSaveResult
{
    public bool Success { get; set; }
    public bool IsDuplicate { get; set; }
    public long TransactionId { get; set; }
    public string Description { get; set; }
    public decimal Value { get; set; }
    public DateTime Date { get; set; }
    public string CreditCard { get; set; }
    public Category? Category { get; set; }
    public string BankSource { get; set; }
    public string Message { get; set; }
    public string ErrorMessage { get; set; }
    public decimal AmmountOfMonth { get; set; }
    public decimal AmmountOfThisCategory { get; set; }
}