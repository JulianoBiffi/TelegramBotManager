using Microsoft.Extensions.DependencyInjection;
using TelegramBotManager.Application.Interfaces;
using TelegramBotManager.Application.Parsers;
using TelegramBotManager.Domain.Interfaces;
using TelegramBotManager.Infrastructure.Repositories;

namespace TelegramBotManager.Infrastructure;

public static class DependencyInjection
{
    public static void AddDependencyInjection(this IServiceCollection services)
    {
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ICreditCardClosingDateRepository, CreditCardClosingDateRepository>();
        
        services.AddScoped<ITelegramMessageRouter, TelegramBotManager.Application.Services.TelegramMessageRouter>();

        // Bank notification parsers
        services.AddScoped<IBankTransactionParser, NubankTransactionParser>();
        services.AddScoped<IBankTransactionParser, PortoTransactionParser>();
        services.AddScoped<IBankTransactionParser, SwileTransactionParser>();
        //services.AddScoped<IBankTransactionParser, BBTransactionParser>();
        //services.AddScoped<IBankTransactionParser, GoogleWalletTransactionParser>();
    }
}
