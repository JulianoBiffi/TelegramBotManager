using System.Globalization;
using System.Text.RegularExpressions;
using TelegramBotManager.Application.DTOs;
using TelegramBotManager.Application.Interfaces;

namespace TelegramBotManager.Application.Parsers;

public class GoogleWalletTransactionParser : IBankTransactionParser
{
    public string BankName => "Google Wallet";

    public bool CanParse(string message)
    {
        if (string.IsNullOrEmpty(message))
            return false;

        var lowerMessage = message.ToLower();
        return (lowerMessage.Contains("google pay") || lowerMessage.Contains("google wallet") || lowerMessage.Contains("gpay")) &&
               (lowerMessage.Contains("pagamento") ||
                lowerMessage.Contains("payment") ||
                lowerMessage.Contains("transação") ||
                lowerMessage.Contains("transaction"));
    }

    public BankTransactionDto Parse(string message)
    {
        var dto = new BankTransactionDto
        {
            BankSource = "googlewallet",
            RawMessage = message,
            IsValid = false,
            TransactionType = "purchase"
        };

        try
        {
            // Extrair valor - Google Wallet pode usar $ ou R$
            var valueMatch = Regex.Match(message, @"(?:R\$|\$)\s?([0-9.,]+)");
            if (valueMatch.Success)
            {
                var valueStr = valueMatch.Groups[1].Value.Replace(".", "").Replace(",", ".");
                dto.Value = decimal.Parse(valueStr, CultureInfo.InvariantCulture);
            }

            // Extrair descrição
            var descriptionPatterns = new[]
            {
                @"(?:at|em|to|para)\s+(.+?)(?:\s+for|\s+R\$|\s+\$|$)",
                @"merchant[:\s]+(.+?)(?:\s+amount|$)",
                @"estabelecimento[:\s]+(.+?)(?:\s+R\$|$)",
                @"payment to\s+(.+?)(?:\s+for|$)"
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

            // Se não encontrou descrição, tentar pegar primeira linha não vazia após o header
            if (string.IsNullOrEmpty(dto.Description))
            {
                var lines = message.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (!line.ToLower().Contains("google") && 
                        !line.ToLower().Contains("payment") && 
                        !line.ToLower().Contains("transaction") &&
                        line.Length > 3)
                    {
                        dto.Description = line.Trim();
                        break;
                    }
                }
            }

            // Tentar detectar qual cartão foi usado
            var cardPatterns = new[]
            {
                @"card ending in\s+(\d{4})",
                @"cartão final\s+(\d{4})",
                @"terminado em\s+(\d{4})"
            };

            string cardEnding = null;
            foreach (var pattern in cardPatterns)
            {
                var cardMatch = Regex.Match(message, pattern, RegexOptions.IgnoreCase);
                if (cardMatch.Success)
                {
                    cardEnding = cardMatch.Groups[1].Value;
                    break;
                }
            }

            dto.CreditCard = !string.IsNullOrEmpty(cardEnding) 
                ? $"Google Wallet (*{cardEnding})" 
                : "Google Wallet";

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