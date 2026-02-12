using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotManager.Application.Features.CreditCardClosingDateManagement;
using TelegramBotManager.Application.Features.CreditCardListClosingDate;
using TelegramBotManager.Application.Features.FinanceControlCreateCategory;
using TelegramBotManager.Application.Features.FinanceControlDefineCategory;
using TelegramBotManager.Application.Features.FinanceControlEditTransactionsOfMonth;
using TelegramBotManager.Application.Features.FinanceControlMessageReceived;
using TelegramBotManager.Application.Features.FinancialControlDailyReports;
using TelegramBotManager.Application.FinancialControl.FinanceControlCreateTransaction;
using TelegramBotManager.Application.FinancialControl.FinanceControlMessageReceived;

namespace TelegramBotManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register Handlers explicitly if needed, but MediatR usually handles this via assembly scanning
        // Listing them here for clarity or manual registration if auto-scanning is disabled/limited
        // In this setup, RegisterServicesFromAssembly covers it.
        
        return services;
    }
}
