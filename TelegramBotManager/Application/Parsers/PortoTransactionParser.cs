using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using TelegramBotManager.Application.DTOs;
using TelegramBotManager.Application.Interfaces;

namespace TelegramBotManager.Application.Parsers;

public class PortoTransactionParser : IBankTransactionParser
{
    public string BankName => "Porto";

    public bool CanParse(string message)
    {
        if (string.IsNullOrEmpty(message))
            return false;

        string textToSearch = message.ToLower();
        try
        {
            var json = JObject.Parse(message);
            textToSearch = (json["texto_completo"]?.ToString() ?? json["texto"]?.ToString() ?? message).ToLower();
            if (json["titulo"]?.ToString()?.ToLower().Contains("porto") == true)
                return true;
        }
        catch { }

        return (textToSearch.Contains("porto") || textToSearch.Contains("porto seguro")) &&
               (textToSearch.Contains("compra aprovada") ||
                textToSearch.Contains("transação") ||
                textToSearch.Contains("pagamento") ||
                textToSearch.Contains("estorno"));
    }

    public BankTransactionDto Parse(string message)
    {
        var dto = new BankTransactionDto
        {
            BankSource = "porto",
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
            // Extrair valor
            var valueMatch = Regex.Match(textToParse, @"R\$\s?([0-9.,]+)");
            if (valueMatch.Success)
            {
                var valueStr = valueMatch.Groups[1].Value.Replace(".", "").Replace(",", ".");
                dto.Value = decimal.Parse(valueStr, CultureInfo.InvariantCulture);
            }

            // Extrair descrição
            var descriptionPatterns = new[]
            {
                @"(?<=às \d{2}:\d{2} em\s)(.*)",
                @"(?:em|no)\s+(.+?)(?:\s+foi|\s+no|\s+R\$|\s+às)",
                @"(?:em|no)\s+(.+?)(?:\s+foi|\s+no|\s+R\$|\s+às)",
                @"local[:\s]+(.+?)(?:\s+R\$|$)",
                @"estabelecimento[:\s]+(.+?)(?:\s+R\$|$)"
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

            if (string.IsNullOrEmpty(dto.Description))
            {
                var lines = textToParse.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 1)
                    dto.Description = lines[1].Trim();
            }

            // Detectar cartão
            if (textToParse.ToLower().Contains("crédito"))
                dto.CreditCard = "Porto Credito";
            else if (textToParse.ToLower().Contains("débito"))
                dto.CreditCard = "Porto Debito";
            else
                dto.CreditCard = "Porto";

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