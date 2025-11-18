using System.Diagnostics;

namespace TelegramBotManager.Common.Helpers;

public static class FileHelper
{
    public static string TempPath
        => Path.GetTempPath();

    public static string AppContextPath =>
        AppContext.BaseDirectory;

    public static async Task SafeDeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception)
        {
        }
    }
}
