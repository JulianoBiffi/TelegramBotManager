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
            // Padrão: "... em 03/02 às 17:46 em SHOPEE DIJECOMMERCE."
            // Regex procura por "em [MERCHANT]." no final da frase, precedido por horário
            var descriptionPatterns = new[]
            {
                @"às \d{2}:\d{2} em (.+?)\.$", 
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
                    // Remove ponto final se tiver sobrado
                    if (dto.Description.EndsWith("."))
                        dto.Description = dto.Description.TrimEnd('.');
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
                // Timestamp vem em milissegundos UTC. Converter para UTC-3 (Brasília)
                var utcDate = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
                dto.Date = utcDate.AddHours(-3);
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