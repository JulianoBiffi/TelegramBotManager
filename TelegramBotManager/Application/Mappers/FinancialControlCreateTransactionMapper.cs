using TelegramBotManager.Application.DTOs;
using TelegramBotManager.Common.Exceptions;
using TelegramBotManager.Common.Helpers;

namespace TelegramBotManager.Application.Mappers;

public static class FinancialControlCreateTransactionMapper
{
    public static FinancialControlCreateTransaction ToDto(this string currentText)
    {
        var chunckOfLines =
            currentText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(l => l.Trim())
                       .ToArray();

        if (chunckOfLines.Length < 7 || chunckOfLines.Length > 7)
            throw new TelegramException(Common.Enum.TelegramInvalidOptionExceptionEnum.InvalidTransactionFormat, "Texto de lançamento em formato inválido.");

        var currentDTO =
            new FinancialControlCreateTransaction
            {
                BotUsername = chunckOfLines[0],
                Command = chunckOfLines[1]
            };

        var dateText =
            chunckOfLines[2].Split(':')[1]
                            .Trim();

        currentDTO.Date = dateText.TryTakeDate();

        currentDTO.CreditCard =
            chunckOfLines[3].Split(':')[1];

        var valueText =
            chunckOfLines[4].Split(':')[1]
                            .Replace(".", ",")
                            .Trim();

        currentDTO.Value =
            decimal.TryParse(valueText, out decimal value)
            ? value
            : 0;

        currentDTO.Description =
            chunckOfLines[5].Split(':')[1];


        string parcelNumberText =
            chunckOfLines[6].Split(':')[1]
                            .Trim();
        currentDTO.ParcelNumber =
            int.TryParse(parcelNumberText, out int number)
            ? number
            : null;

        return currentDTO;
    }
}