using TelegramBotManager.Domain.Entities.FinancialControl;
using TelegramBotManager.Infrastructure.Persistence.Models;

namespace TelegramBotManager.Infrastructure.Repositories.Mappers;

public static class CategoryMapper
{
    public static Category ToDomain(CategoryModel model)
    {
        if (model == null) return null;

        var entity = new Category(model.Description);
        entity.SetId(model.Id);
        return entity;
    }

    public static CategoryModel ToModel(Category entity)
    {
        if (entity == null) return null;

        return new CategoryModel
        {
            Id = entity.Id,
            Description = entity.Description
        };
    }
}
