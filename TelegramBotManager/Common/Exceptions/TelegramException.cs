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

    public TelegramException(string customMessage, TelegramBotTypeEnum telegramBotTypeEnum = TelegramBotTypeEnum.FinancialControl)
    {
        TelegramBotType = telegramBotTypeEnum;
        TelegramMessage = customMessage;
    }
}