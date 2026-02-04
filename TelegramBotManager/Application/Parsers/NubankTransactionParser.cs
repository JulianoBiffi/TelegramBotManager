using System.Globalization;
using System.Text.RegularExpressions;
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

        var lowerMessage = message.ToLower();
        return lowerMessage.Contains("nubank") &&
               (lowerMessage.Contains("compra aprovada") ||
                lowerMessage.Contains("compra no débito") ||
                lowerMessage.Contains("compra no crédito") ||
                lowerMessage.Contains("estorno"));
    }

    public BankTransactionDto Parse(string message)
    {
        var dto = new BankTransactionDto
        {
            BankSource = "nubank",
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
            else if (message.ToLower().Contains("compra"))
            {
                dto.TransactionType = "purchase";
            }

            // Extrair valor - padrões: R$ 123,45 ou R$123,45
            var valueMatch = Regex.Match(message, @"R\$\s?([0-9.,]+)");
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
                var descMatch = Regex.Match(message, pattern, RegexOptions.IgnoreCase);
                if (descMatch.Success)
                {
                    dto.Description = descMatch.Groups[1].Value.Trim();
                    break;
                }
            }

            // Se não encontrou descrição específica, pegar parte da mensagem
            if (string.IsNullOrEmpty(dto.Description))
            {
                var lines = message.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 1)
                    dto.Description = lines[1].Trim();
            }

            // Detectar cartão
            if (message.ToLower().Contains("crédito"))
                dto.CreditCard = "Nubank Credito";
            else if (message.ToLower().Contains("débito"))
                dto.CreditCard = "Nubank Debito";
            else
                dto.CreditCard = "Nubank";

            // Data da transação (usar data atual se não especificada)
            dto.Date = DateTime.Now;

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