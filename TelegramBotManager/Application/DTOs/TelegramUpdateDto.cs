using Newtonsoft.Json;

namespace TelegramBotManager.Application.DTOs;

public class TelegramUpdateDto
{
    [JsonProperty("update_id")]
    public long UpdateId { get; set; }

    [JsonProperty("message")]
    public TelegramMessageDto? Message { get; set; }

    [JsonProperty("callback_query")]
    public TelegramCallbackQueryDto? CallbackQuery { get; set; }
}

public class TelegramCallbackQueryDto
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("from")]
    public TelegramUserDto? From { get; set; }

    [JsonProperty("message")]
    public TelegramMessageDto? Message { get; set; }

    [JsonProperty("chat_instance")]
    public string ChatInstance { get; set; } = string.Empty;

    [JsonProperty("data")]
    public string? Data { get; set; }
}

public class TelegramMessageDto
{
    [JsonProperty("message_id")]
    public int MessageId { get; set; }

    [JsonProperty("from")]
    public TelegramUserDto? From { get; set; }

    [JsonProperty("chat")]
    public TelegramChatDto? Chat { get; set; }

    [JsonProperty("date")]
    public long Date { get; set; }

    [JsonProperty("text")]
    public string? Text { get; set; }

    [JsonProperty("reply_markup")]
    public TelegramInlineKeyboardMarkupDto? ReplyMarkup { get; set; }
}

public class TelegramUserDto
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("is_bot")]
    public bool IsBot { get; set; }

    [JsonProperty("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonProperty("last_name")]
    public string? LastName { get; set; }

    [JsonProperty("username")]
    public string? Username { get; set; }

    [JsonProperty("language_code")]
    public string? LanguageCode { get; set; }
}

public class TelegramChatDto
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("first_name")]
    public string? FirstName { get; set; }

    [JsonProperty("last_name")]
    public string? LastName { get; set; }

    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;
}

public class TelegramInlineKeyboardMarkupDto
{
    [JsonProperty("inline_keyboard")]
    public List<List<TelegramInlineKeyboardButtonDto>> InlineKeyboard { get; set; }
        = new();
}

public class TelegramInlineKeyboardButtonDto
{
    [JsonProperty("text")]
    public string Text { get; set; } = string.Empty;

    [JsonProperty("switch_inline_query_current_chat")]
    public string? SwitchInlineQueryCurrentChat { get; set; }

    [JsonProperty("callback_data")]
    public string? CallbackData { get; set; }
}
