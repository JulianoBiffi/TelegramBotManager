using TelegramBotManager.Domain.Entities.FinancialControl;

namespace TelegramBotManager.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<category> GetCategory(long id, CancellationToken cancellationToken);
    Task<List<category>> GetAllCategoriesAsync(CancellationToken cancellationToken);
    Task<category> SaveAsync(category category, CancellationToken cancellationToken);
    Task<category> GetCategoryByTranscationDesciption(string transactionDescription);
}