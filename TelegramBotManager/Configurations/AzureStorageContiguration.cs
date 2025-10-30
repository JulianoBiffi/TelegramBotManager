using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramBotManager.Configurations;

public static class AzureStorageContiguration
{
    public static void AddAzureStorageAndQueue(this IServiceCollection services, IConfiguration configuration)
    {
        var azureStorageConnection =
            configuration.GetValue<string>("Azure_StorageConnectionString");

        if (string.IsNullOrEmpty(azureStorageConnection))
            throw new Exception("AzureStorageCloudStorageConnectionString is not set in the configuration.");

        services.AddSingleton(new BlobServiceClient(azureStorageConnection));

        var financialControlQueueName =
            configuration.GetValue<string>("Azure_QueueFinancialControlName");

        services.AddKeyedSingleton("FinancialQueueClient", (sp, key) =>
        {
            return new QueueClient(
                azureStorageConnection,
                financialControlQueueName,
                new QueueClientOptions
                {
                    MessageEncoding = QueueMessageEncoding.Base64
                });
        });
    }
}
