namespace TelegramBotManager.Common.Helpers;

public static class FileHelper
{
    public static string GetFolderPath =>
        Path.GetTempPath();

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
