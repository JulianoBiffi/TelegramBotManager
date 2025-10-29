using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TelegramBotManager.Configurations;
using TelegramBotManager.Infrastructure;
using TelegramBotManager.Middleware;

var builder =
    FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Configuration
       .SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
       .AddEnvironmentVariables();

builder.Services
       .AddApplicationInsightsTelemetryWorkerService()
       .ConfigureFunctionsApplicationInsights();

builder.UseMiddleware<ValidationExceptionMiddleware>();

builder.UseMiddleware<TelegramExceptionMiddleware>();

IConfiguration configuration = builder.Configuration;

builder.Services.AddAzureStorageAndQueue(configuration);

builder.Services.AddTelegramBotClient(configuration);

builder.Services.AddFinancialControlOptions(configuration);

builder.Services.AddDependencyInjection();

await builder.Services.AddSupabaseConfigurations(configuration);

//builder.Services.AddAutoMapper(configAction => { }, typeof(Program));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

builder.Build().Run();