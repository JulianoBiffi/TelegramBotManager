using System.Globalization;
using System.Text.RegularExpressions;
using TelegramBotManager.Application.DTOs;
using TelegramBotManager.Application.Interfaces;
using TelegramBotManager.Common.Helpers;

namespace TelegramBotManager.Application.Parsers;

public class GoogleWalletTransactionParser : IBankTransactionParser
{
    public string BankName => "Google Wallet";
    public string PackageName => "com.google.android.apps.walletnfcrel";

    public bool CanParse(PurchaseDto purchaseDto)
    {
        string textToSearch =
            string.Concat(purchaseDto.Package, "_", purchaseDto.Title, "_", purchaseDto.FullText)
                  .ToLower();

        return
            textToSearch.Contains(BankName.ToLower()) ||
                textToSearch.Contains(PackageName.ToLower());
    }

    public BankTransactionDto Parse(PurchaseDto purchaseDto)
    {
        var dto =
            new BankTransactionDto
            {
                BankSource = "Google Wallet",
                CreditCard = "NUBANK",
                Date = purchaseDto.Timestamp.ConvertUnixToBrazil(),
                IsValid = false
            };

        string textToParse = purchaseDto.FullText;

        try
        {
            var valueMatch = Regex.Match(textToParse, @"R\$\s?([0-9.,]+)");
            if (valueMatch.Success)
            {
                var valueStr = valueMatch.Groups[1].Value.Replace(".", "").Replace(",", ".");
                dto.Value = decimal.Parse(valueStr, CultureInfo.InvariantCulture);
            }

            var descriptionPatterns =
                new[]
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

            dto.IsValid = dto.Value > 0 && !string.IsNullOrEmpty(dto.Description);
        }
        catch
        {
            dto.IsValid = false;
        }

        return dto;
    }
}