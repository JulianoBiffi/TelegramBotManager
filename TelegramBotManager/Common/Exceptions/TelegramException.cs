using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotManager.Common.Enum;

namespace TelegramBotManager.Common.Exceptions;

public class TelegramException : Exception
{
    public TelegramBotTypeEnum TelegramBotType { get; private set; }
    public string TelegramMessage { get; private set; }
    public TelegramInvalidOptionExceptionEnum? TelegramInvalidOption { get; private set; } = null;

    public TelegramException(string customMessage, TelegramBotTypeEnum telegramBotTypeEnum = TelegramBotTypeEnum.FinancialControl)
    {
        TelegramBotType = telegramBotTypeEnum;
        TelegramMessage = customMessage;
    }

    public TelegramException(Exception exception, TelegramBotTypeEnum telegramBotTypeEnum = TelegramBotTypeEnum.FinancialControl)
    {
        TelegramBotType = telegramBotTypeEnum;
        TelegramMessage = exception.Message;
    }

    public TelegramException(string? message, Exception? innerException) : base(message, innerException)
    {
        TelegramMessage = innerException?.Message;
        TelegramBotType = TelegramBotTypeEnum.FinancialControl;
    }

    public TelegramException(TelegramInvalidOptionExceptionEnum typeOfOption, string message)
    {
        TelegramBotType = TelegramBotTypeEnum.FinancialControl;
        TelegramInvalidOption = typeOfOption;
        TelegramMessage = message;
    }
}