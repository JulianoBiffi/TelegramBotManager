using Microsoft.Extensions.DependencyInjection;
using TelegramBotManager.Domain.Interfaces;
using TelegramBotManager.Infrastructure.Repositories;

namespace TelegramBotManager.Infrastructure;

public static class DependencyInjection
{
    public static void AddDependencyInjection(this IServiceCollection services)
    {
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
    }
}
