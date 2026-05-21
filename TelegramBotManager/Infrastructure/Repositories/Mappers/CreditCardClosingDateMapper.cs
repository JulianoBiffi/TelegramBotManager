using TelegramBotManager.Domain.Entities.FinancialControl;
using TelegramBotManager.Infrastructure.Persistence.Models;

namespace TelegramBotManager.Infrastructure.Repositories.Mappers;

public static class CreditCardClosingDateMapper
{
    public static CreditCardClosingDate ToDomain(CreditCardClosingDateModel? model)
    {
        if (model == null)
            return null;

        var entity = new CreditCardClosingDate(
            model.BankName,
            model.ClosingDate,
            model.BestDayToBuy
        );

        entity.SetId(model.Id);

        return entity;
    }

    public static CreditCardClosingDateModel ToModel(CreditCardClosingDate entity)
    {
        return new CreditCardClosingDateModel
        {
            Id = entity.Id,
            BankName = entity.BankName,
            ClosingDate = entity.ClosingDate,
            BestDayToBuy = entity.BestDayToBuy
        };
    }
}
