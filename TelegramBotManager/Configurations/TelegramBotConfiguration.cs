using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace TelegramBotManager.Configurations;

public static class TelegramBotConfiguration
{
    public static void AddTelegramBotClient(this IServiceCollection services, IConfiguration configuration)
    {
        var financialControlToken =
            configuration.GetValue<string>("FinancialControl:TelegramBotToken");

        if (string.IsNullOrEmpty(financialControlToken))
            throw new Exception("FinancialControl:TelegramBotToken is not set in the configuration.");

        services.AddKeyedSingleton("FinancialControl", (sp, key) =>
        {
            return new TelegramBotClient(financialControlToken);
        });
    }
}