using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Supabase;

namespace TelegramBotManager.Configurations;

public static class SupabaseConfiguration
{
    public static async Task AddSupabaseConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        var supabaseUrl = configuration["SupabaseUrl"];
        var supabaseKey = configuration["SupabaseApiKey"];

        if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
        {
            throw new ArgumentNullException("Supabase configuration is missing");
        }

        var supabaseOptions =
            new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true,
                Schema = "financialcontrol"
            };

        var supabaseClient =
            new Supabase.Client(supabaseUrl, supabaseKey, supabaseOptions);

        await supabaseClient.InitializeAsync();

        services.AddSingleton(supabaseClient);
    }
}
