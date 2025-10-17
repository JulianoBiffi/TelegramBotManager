using System.Globalization;
using TelegramBotManager.Common.Exceptions;

namespace TelegramBotManager.Application.DTOs;

public class FinancialControlCreate
{
    public string BotUsername { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string CreditCard { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Category { get; set; } = string.Empty;
}

public static class FinancialControlCreateRecordParser
{
    public static FinancialControlCreate ToDto(this string currentText)
    {
        var chunckOfLines =
            currentText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(l => l.Trim())
                       .ToArray();

        if (chunckOfLines.Length < 6 || chunckOfLines.Length > 6)
            throw new TelegramException("Texto de lançamento em formato inválido.");

        var currentDTO =
            new FinancialControlCreate
            {
                BotUsername = chunckOfLines[0],
                Command = chunckOfLines[1]
            };

        var dateText =
            chunckOfLines[2].Substring("Data".Length)
                            .Trim(' ', '(', ')', ':');

        currentDTO.Date = dateText.Length switch
        {
            1 => DateTime.Parse($"0{dateText}/{DateTime.Now.Month}/{DateTime.Now.Year}", CultureInfo.GetCultureInfo("pt-BR")),
            2 => DateTime.Parse($"{dateText}/{DateTime.Now.Month}/{DateTime.Now.Year}", CultureInfo.GetCultureInfo("pt-BR")),
            _ => DateTime.Today
        };

        currentDTO.CreditCard =
            chunckOfLines[3].Substring("Cartão".Length)
                            .Trim(' ', '(', ')', ':');

        var valueText =
            chunckOfLines[4].Substring("Valor".Length)
                            .Trim(' ', ':');

        currentDTO.Value =
            decimal.Parse(valueText, NumberStyles.Currency, CultureInfo.GetCultureInfo("pt-BR"));

        currentDTO.Category =
            chunckOfLines[5].Substring("Categoria".Length)
                            .Trim(' ', ':');

        return currentDTO;
    }
}