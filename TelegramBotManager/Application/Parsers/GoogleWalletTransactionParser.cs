using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
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

        string textToSearch = message.ToLower();
        try
        {
            var json = JObject.Parse(message);
            textToSearch = (json["texto_completo"]?.ToString() ?? json["texto"]?.ToString() ?? message).ToLower();
            if (json["titulo"]?.ToString()?.ToLower().Contains("google") == true || 
                json["package"]?.ToString()?.Contains("google") == true)
                return true;
        }
        catch { }

        return (textToSearch.Contains("google pay") || textToSearch.Contains("google wallet") || textToSearch.Contains("gpay")) &&
               (textToSearch.Contains("pagamento") ||
                textToSearch.Contains("payment") ||
                textToSearch.Contains("transação") ||
                textToSearch.Contains("transaction"));
    }

    public BankTransactionDto Parse(string message)
    {
        var dto = new BankTransactionDto
        {
            BankSource = "googlewallet",
            IsValid = false
        };

        string textToParse = message;
        long timestamp = 0;

        try
        {
            var json = JObject.Parse(message);
            textToParse = json["texto_completo"]?.ToString() ?? json["texto"]?.ToString() ?? message;
            if (json["timestamp"] != null)
                timestamp = (long)json["timestamp"];
        }
        catch { }

        try
        {
            // Extrair valor - Google Wallet pode usar $ ou R$
            var valueMatch = Regex.Match(textToParse, @"(?:R\$|\$)\s?([0-9.,]+)");
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
                var descMatch = Regex.Match(textToParse, pattern, RegexOptions.IgnoreCase);
                if (descMatch.Success)
                {
                    dto.Description = descMatch.Groups[1].Value.Trim();
                    break;
                }
            }

            // Se não encontrou descrição, tentar pegar primeira linha não vazia após o header
            if (string.IsNullOrEmpty(dto.Description))
            {
                var lines = textToParse.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
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
                var cardMatch = Regex.Match(textToParse, pattern, RegexOptions.IgnoreCase);
                if (cardMatch.Success)
                {
                    cardEnding = cardMatch.Groups[1].Value;
                    break;
                }
            }

            dto.CreditCard = !string.IsNullOrEmpty(cardEnding) 
                ? $"Google Wallet (*{cardEnding})" 
                : "Google Wallet";

            // Data
            if (timestamp > 0)
            {
                dto.Date = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
            }
            else
            {
                dto.Date = DateTime.Now;
            }

            dto.IsValid = dto.Value > 0 && !string.IsNullOrEmpty(dto.Description);
        }
        catch
        {
            dto.IsValid = false;
        }

        return dto;
    }
}