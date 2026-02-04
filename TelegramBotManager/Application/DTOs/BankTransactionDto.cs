namespace TelegramBotManager.Application.DTOs;

public class BankTransactionDto
{
    public string Description { get; set; }
    public decimal Value { get; set; }
    public DateTime Date { get; set; }
    public string CreditCard { get; set; }
    public string TransactionType { get; set; } // "purchase", "refund", "payment"
    public string BankSource { get; set; } // "nubank", "porto", "bb", "googlewallet"
    public bool IsValid { get; set; }
    public string RawMessage { get; set; }
}