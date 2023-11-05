using System.Text.Json.Serialization;

namespace WispoRoboto.Chatbot.Telegram.Models;

public class Chat
{
    [JsonPropertyName("id")]
    public required long Id { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
