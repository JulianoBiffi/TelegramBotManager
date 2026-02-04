using System.Globalization;
using System.Text.RegularExpressions;
using TelegramBotManager.Application.DTOs;
using TelegramBotManager.Application.Interfaces;

namespace TelegramBotManager.Application.Parsers;

public class BBTransactionParser : IBankTransactionParser
{
    public string BankName => "Banco do Brasil";

    public bool CanParse(string message)
    {
        if (string.IsNullOrEmpty(message))
            return false;

        var lowerMessage = message.ToLower();
        return (lowerMessage.Contains("banco do brasil") || lowerMessage.Contains(" bb ") || lowerMessage.Contains("ourocard")) &&
               (lowerMessage.Contains("compra") ||
                lowerMessage.Contains("transação") ||
                lowerMessage.Contains("pagamento") ||
                lowerMessage.Contains("estorno"));
    }

    public BankTransactionDto Parse(string message)
    {
        var dto = new BankTransactionDto
        {
            BankSource = "bb",
            RawMessage = message,
            IsValid = false
        };

        try
        {
            // Detectar tipo de transação
            if (message.ToLower().Contains("estorno"))
            {
                dto.TransactionType = "refund";
            }
            else if (message.ToLower().Contains("compra") || message.ToLower().Contains("transação"))
            {
                dto.TransactionType = "purchase";
            }
            else if (message.ToLower().Contains("pagamento"))
            {
                dto.TransactionType = "payment";
            }

            // Extrair valor
            var valueMatch = Regex.Match(message, @"R\$\s?([0-9.,]+)");
            if (valueMatch.Success)
            {
                var valueStr = valueMatch.Groups[1].Value.Replace(".", "").Replace(",", ".");
                dto.Value = decimal.Parse(valueStr, CultureInfo.InvariantCulture);
            }

            // Extrair descrição
            var descriptionPatterns = new[]
            {
                @"(?:em|no)\s+(.+?)(?:\s+foi|\s+no|\s+R\$|\s+às)",
                @"estabelecimento[:\s]+(.+?)(?:\s+valor|\s+R\$|$)",
                @"local[:\s]+(.+?)(?:\s+R\$|$)"
            };

            foreach (var pattern in descriptionPatterns)
            {
                var descMatch = Regex.Match(message, pattern, RegexOptions.IgnoreCase);
                if (descMatch.Success)
                {
                    dto.Description = descMatch.Groups[1].Value.Trim();
                    break;
                }
            }

            if (string.IsNullOrEmpty(dto.Description))
            {
                var lines = message.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 1)
                    dto.Description = lines[1].Trim();
            }

            // Detectar cartão
            if (message.ToLower().Contains("ourocard") || message.ToLower().Contains("crédito"))
                dto.CreditCard = "BB Ourocard";
            else if (message.ToLower().Contains("débito"))
                dto.CreditCard = "BB Debito";
            else
                dto.CreditCard = "BB";

            dto.Date = DateTime.Now;
            dto.IsValid = dto.Value > 0 && !string.IsNullOrEmpty(dto.Description);
        }
        catch
        {
            dto.IsValid = false;
        }

        return dto;
    }
}