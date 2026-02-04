using TelegramBotManager.Application.DTOs;

namespace TelegramBotManager.Application.Interfaces;

public interface IBankTransactionParser
{
    string BankName { get; }
    bool CanParse(string message);
    BankTransactionDto Parse(string message);
}