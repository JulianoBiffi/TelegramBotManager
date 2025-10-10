using Microsoft.AspNetCore.Http;

namespace TelegramBotManager.Common.Helpers;

public static class HttpRequestHelper
{
    public static async Task<string> GetBodyAsStringAsync(this HttpRequest req, CancellationToken cancellationToken = default)
    {
        using (var reader = new StreamReader(req.Body))
        {
            return await reader.ReadToEndAsync(cancellationToken);
        }
    }
}
