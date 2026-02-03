using TelegramBotManager.Domain.Entities.FinancialControl;

namespace TelegramBotManager.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<Category> GetCategory(long id, CancellationToken cancellationToken);
    Task<List<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken);
    Task<Category> SaveAsync(Category category, CancellationToken cancellationToken);
    Task<Category> GetCategoryByTranscationDesciption(string transactionDescription);
}