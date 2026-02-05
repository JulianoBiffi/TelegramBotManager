using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using TelegramBotManager.Application.DTOs;
using TelegramBotManager.Application.Interfaces;

namespace TelegramBotManager.Application.Parsers;

public class NubankTransactionParser : IBankTransactionParser
{
    public string BankName => "Nubank";

    public bool CanParse(string message)
    {
        if (string.IsNullOrEmpty(message))
            return false;

        string textToSearch = message.ToLower();
        try
        {
            var json = JObject.Parse(message);
            textToSearch = (json["texto_completo"]?.ToString() ?? json["texto"]?.ToString() ?? message).ToLower();
            if (json["titulo"]?.ToString()?.ToLower().Contains("nubank") == true || 
                json["package"]?.ToString()?.Contains("com.nu") == true)
                return true;
        }
        catch { }

        return (textToSearch.Contains("nubank") || textToSearch.Contains("nu pagamentos")) &&
               (textToSearch.Contains("compra aprovada") ||
                textToSearch.Contains("compra no débito") ||
                textToSearch.Contains("compra no crédito") ||
                textToSearch.Contains("estorno"));
    }

    public BankTransactionDto Parse(string message)
    {
        var dto = new BankTransactionDto
        {
            BankSource = "nubank",
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
            // Extrair valor - padrões: R$ 123,45 ou R$123,45
            var valueMatch = Regex.Match(textToParse, @"R\$\s?([0-9.,]+)");
            if (valueMatch.Success)
            {
                var valueStr = valueMatch.Groups[1].Value.Replace(".", "").Replace(",", ".");
                dto.Value = decimal.Parse(valueStr, CultureInfo.InvariantCulture);
            }

            // Extrair descrição/estabelecimento
            var descriptionPatterns = new[]
            {
                @"(?:em|no)\s+(.+?)(?:\s+foi|\s+no|\s+R\$)",
                @"estabelecimento\s+(.+?)(?:\s+no|\s+R\$|$)"
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

            // Se não encontrou descrição específica, pegar parte da mensagem
            if (string.IsNullOrEmpty(dto.Description))
            {
                var lines = textToParse.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 1)
                    dto.Description = lines[1].Trim();
            }

            // Detectar cartão
            if (textToParse.ToLower().Contains("crédito"))
                dto.CreditCard = "Nubank Credito";
            else if (textToParse.ToLower().Contains("débito"))
                dto.CreditCard = "Nubank Debito";
            else
                dto.CreditCard = "Nubank";

            // Data
            if (timestamp > 0)
            {
                dto.Date = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
            }
            else
            {
                dto.Date = DateTime.Now;
            }

            // Validar se conseguimos extrair informações essenciais
            dto.IsValid = dto.Value > 0 && !string.IsNullOrEmpty(dto.Description);
        }
        catch
        {
            dto.IsValid = false;
        }

        return dto;
    }
}