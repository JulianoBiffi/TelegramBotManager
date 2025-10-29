using System.Globalization;
using TelegramBotManager.Common.Exceptions;

namespace TelegramBotManager.Application.DTOs;

public class FinancialControlCreateTransaction
{
    public string BotUsername { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string CreditCard { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Description { get; set; } = string.Empty;
}