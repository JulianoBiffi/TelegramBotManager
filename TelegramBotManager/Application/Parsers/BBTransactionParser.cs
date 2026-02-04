using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
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

        string textToSearch = message.ToLower();
        try
        {
            var json = JObject.Parse(message);
            textToSearch = (json["texto_completo"]?.ToString() ?? json["texto"]?.ToString() ?? message).ToLower();
            if (json["titulo"]?.ToString()?.ToLower().Contains("bb") == true || 
                json["package"]?.ToString()?.Contains("br.com.bb") == true)
                return true;
        }
        catch { }

        return (textToSearch.Contains("banco do brasil") || textToSearch.Contains(" bb ") || textToSearch.Contains("ourocard")) &&
               (textToSearch.Contains("compra") ||
                textToSearch.Contains("transação") ||
                textToSearch.Contains("pagamento") ||
                textToSearch.Contains("estorno"));
    }

    public BankTransactionDto Parse(string message)
    {
        var dto = new BankTransactionDto
        {
            BankSource = "bb",
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
                @"(?:em|no)\s+(.+?)(?:\s+foi|\s+no|\s+R\$|\s+às)",
                @"estabelecimento[:\s]+(.+?)(?:\s+valor|\s+R\$|$)",
                @"local[:\s]+(.+?)(?:\s+R\$|$)"
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
            if (textToParse.ToLower().Contains("ourocard") || textToParse.ToLower().Contains("crédito"))
                dto.CreditCard = "BB Ourocard";
            else if (textToParse.ToLower().Contains("débito"))
                dto.CreditCard = "BB Debito";
            else
                dto.CreditCard = "BB";

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