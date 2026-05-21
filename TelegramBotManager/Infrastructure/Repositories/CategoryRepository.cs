using Supabase;
using TelegramBotManager.Domain.Entities.FinancialControl;
using TelegramBotManager.Domain.Interfaces;
using TelegramBotManager.Infrastructure.Persistence.Models;
using TelegramBotManager.Infrastructure.Repositories.Mappers;

namespace TelegramBotManager.Infrastructure.Repositories;

public class CategoryRepository : BaseRepository<CategoryModel>, ICategoryRepository
{
    public CategoryRepository(Client supabaseClient) : base(supabaseClient)
    {
    }

    public async Task<List<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken)
    {
        var models = await GetAllAsync(cancellationToken);
        return models.Select(CategoryMapper.ToDomain).ToList();
    }

    public async Task<Category> GetCategory(long id, CancellationToken cancellationToken)
    {
        var model = await GetByIdAsync(id, cancellationToken);
        return CategoryMapper.ToDomain(model);
    }

    public async Task<Category> SaveAsync(Category category, CancellationToken cancellationToken)
    {
        var model = CategoryMapper.ToModel(category);

        var result = model.Id > 0
            ? await base.UpdateAsync(model, cancellationToken)
            : await base.InsertAsync(model, cancellationToken);

        return CategoryMapper.ToDomain(result);
    }

    public async Task<Category> GetCategoryByTranscationDesciption(
        string transactionDescription)
    {
        if (string.IsNullOrEmpty(transactionDescription))
            return null;

        var result = await _supabaseClient.Rpc<List<CategoryModel>>(
                    "getcategorybytransactiondescription",
                    new Dictionary<string, object>
                    {
                        { "transactiondescription", transactionDescription.Trim() }
                    });

        var model = result?.FirstOrDefault();
        return CategoryMapper.ToDomain(model);
    }
}
