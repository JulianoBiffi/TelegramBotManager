using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramBotManager.Configurations;

public static class FinancialControlOptionsConfiguration
{

    public static void AddFinancialControlOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var financialControlToken =
            configuration.GetValue<string>("FinancialControl:TelegramBotToken");

        if (string.IsNullOrEmpty(financialControlToken))
            throw new Exception("FinancialControl:TelegramBotToken is not set in the configuration.");

        var financialAllowedUserIds =
            configuration.GetValue<string>("FinancialControl:AllowedUserIds");

        if (string.IsNullOrEmpty(financialAllowedUserIds))
            throw new Exception("FinancialControl:AllowedUserIds is not set in the configuration.");

        var financialAllowedGroup =
            configuration.GetValue<string>("FinancialControl:AllowedGroup");

        if (string.IsNullOrEmpty(financialAllowedGroup))
            throw new Exception("FinancialControl:AllowedGroup is not set in the configuration.");

        var financialOptions =
            new FinancialControlOptions(
                telegramBotToken: financialControlToken,
                allowedUserIds:
                    financialAllowedUserIds.Split(';')
                                           .Select(x => long.TryParse(x, out long result) ? result : default)
                                           .ToArray(),
                allowedGroup: long.TryParse(financialAllowedGroup, out long result) ? result : default
            );

        services.AddSingleton(financialOptions);
    }
}
