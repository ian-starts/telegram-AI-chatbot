using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace WispoRoboto.Chatbot.Telegram.Models;

public class Message
{
    [JsonPropertyName("message_id")] public required long MessageId { get; set; }

    [JsonPropertyName("from")] public required From From { get; set; }

    [JsonPropertyName("chat")] public required Chat Chat { get; set; }

    [JsonPropertyName("date")] public required int Date { get; set; }

    [JsonPropertyName("text")] public required string Text { get; set; }

    public bool IsCommand => Text.StartsWith("/");

    public string Command
    {
        get
        {
            if (!IsCommand)
            {
                return "";
            }

            var matches = Regex.Matches(Text, @"(\/[a-zA-Z]+) ?(.+)?");
            return matches.First().Groups[1].Value;
        }
    }

    public string Argument
    {
        get
        {
            if (!IsCommand)
            {
                return "";
            }

            var matches = Regex.Matches(Text, @"(\/[a-zA-Z]+) (.+)?");
            if (!matches.Any())
            {
                return "";
            }
            return matches.First().Groups.Count < 3 ? "" : matches.First().Groups[2].Value;
        }
    }
}