using System.Text.Json.Serialization;

namespace WispoRoboto.Chatbot.Telegram.Models;

public class TelegramRequest
{
    [JsonPropertyName("update_id")]
    public required long UpdateId { get; set; }

    [JsonPropertyName("message")]
    public required Message Message { get; set; }
}