using Azure;
using Azure.Data.Tables;
using WispoRoboto.Chatbot.Telegram.Models;

namespace WispoRoboto.Chatbot.AzureTables.Entities;

public class TelegramMessageAzureTables : ITableEntity
{
    public required string PartitionKey { get; set; }

    public required string RowKey { get; set; }

    public required string ChatTitle { get; set; }
    public required DateTime Date { get; set; }
    public required string From { get; set; }
    public required string Text { get; set; }
    public required string MessageId { get; set; }

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }
}