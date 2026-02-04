namespace TelegramBotManager.Application.DTOs;

public class BankTransactionDto
{
    public string Description { get; set; }
    public decimal Value { get; set; }
    public DateTime Date { get; set; }
    public string CreditCard { get; set; }
    public string BankSource { get; set; }
    public bool IsValid { get; set; }
}