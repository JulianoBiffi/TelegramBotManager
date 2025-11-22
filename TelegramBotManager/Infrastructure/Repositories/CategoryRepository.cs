using Supabase;
using TelegramBotManager.Domain.Entities.FinancialControl;
using TelegramBotManager.Domain.Interfaces;

namespace TelegramBotManager.Infrastructure.Repositories;

public class CategoryRepository : BaseRepository<category>, ICategoryRepository
{
    public CategoryRepository(Client supabaseClient) : base(supabaseClient)
    {
    }

    public async Task<List<category>> GetAllCategoriesAsync(CancellationToken cancellationToken)
        => await GetAllAsync(cancellationToken);

    public async Task<category> GetCategory(long id, CancellationToken cancellationToken)
        => await GetByIdAsync(id, cancellationToken);

    public async Task<category> SaveAsync(category category, CancellationToken cancellationToken)
    { 
        return category.Id > 0
            ? await base.UpdateAsync(category, cancellationToken)
            : await base.InsertAsync(category, cancellationToken);
    }

    public async Task<category> GetCategoryByTranscationDesciption(
        string transactionDescription)
    {
        if (string.IsNullOrEmpty(transactionDescription))
            return null;

        return
            (await _supabaseClient.Rpc<List<category>>(
                    "getcategorybytransactiondescription",
                    new Dictionary<string, object>
                    {
                        { "transactiondescription", transactionDescription.Trim() }
                    })
            ).FirstOrDefault();
    }
}
