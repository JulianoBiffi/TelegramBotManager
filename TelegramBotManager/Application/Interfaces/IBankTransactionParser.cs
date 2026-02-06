using TelegramBotManager.Application.DTOs;

namespace TelegramBotManager.Application.Interfaces;

public interface IBankTransactionParser
{
    string BankName { get; }
    string PackageName { get; }
    bool CanParse(PurchaseDto purchaseDto);
    BankTransactionDto Parse(PurchaseDto purchaseDto);
}