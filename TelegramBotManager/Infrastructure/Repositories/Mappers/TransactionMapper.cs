using TelegramBotManager.Domain.Entities.FinancialControl;
using TelegramBotManager.Infrastructure.Persistence.Models;

namespace TelegramBotManager.Infrastructure.Repositories.Mappers;

public static class TransactionMapper
{
    public static Transaction ToDomain(TransactionModel model)
    {
        if (model == null) return null;

        var entity = new Transaction(
            model.Description,
            model.Value,
            model.Date,
            model.CreditCard,
            model.CategoryId,
            model.ParcelNumber
        );
        entity.SetId(model.Id); // Assuming Id matches

        if (model.Category != null)
        {
            entity.SetCategory(CategoryMapper.ToDomain(model.Category));
        }

        return entity;
    }

    public static TransactionModel ToModel(Transaction entity)
    {
        if (entity == null) return null;

        return new TransactionModel
        {
            Id = entity.Id,
            Description = entity.Description,
            Value = entity.Value,
            Date = entity.Date,
            CreditCard = entity.CreditCard,
            CategoryId = entity.CategoryId,
            ParcelNumber = entity.ParcelNumber,
            Category = CategoryMapper.ToModel(entity.Category)
        };
    }
}
