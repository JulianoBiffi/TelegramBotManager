using TelegramBotManager.Domain.Entities.FinancialControl;
using TelegramBotManager.Domain.ValueObjects;
using TelegramBotManager.Infrastructure.Persistence.Models;

namespace TelegramBotManager.Infrastructure.Repositories.Mappers;

public static class TransactionMapper
{
    public static Transaction ToDomain(TransactionModel model)
    {
        if (model == null) return null;

        var entity = new Transaction(
            model.Description,
            new Money(model.Value),
            model.Date,
            new CreditCard(model.CreditCard),
            model.CategoryId,
            model.ParcelNumber
        )
        {
            Id = model.Id // Possible thanks to internal set
        };

        if (model.CategoryId.HasValue && model.Category?.Description != null)
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
            Value = entity.Value, // Implicit operator Money -> decimal
            Date = entity.Date,
            CreditCard = entity.CreditCard, // Implicit operator CreditCard -> string
            CategoryId = entity.CategoryId,
            ParcelNumber = entity.ParcelNumber,
            Category = CategoryMapper.ToModel(entity.Category)
        };
    }
}
