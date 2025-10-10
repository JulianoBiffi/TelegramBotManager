using System.Text.Json.Serialization;

namespace TelegramBotManager.Application.DTOs;

public class TelegramUpdateDto
{
    [JsonPropertyName("update_id")]
    public long UpdateId { get; set; }

    [JsonPropertyName("message")]
    public TelegramMessageDto? Message { get; set; }
}

public class TelegramMessageDto
{
    [JsonPropertyName("message_id")]
    public int MessageId { get; set; }

    [JsonPropertyName("from")]
    public TelegramUserDto? From { get; set; }

    [JsonPropertyName("chat")]
    public TelegramChatDto? Chat { get; set; }

    [JsonPropertyName("date")]
    public long Date { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public class TelegramUserDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("is_bot")]
    public bool IsBot { get; set; }

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("language_code")]
    public string? LanguageCode { get; set; }
}

public class TelegramChatDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}