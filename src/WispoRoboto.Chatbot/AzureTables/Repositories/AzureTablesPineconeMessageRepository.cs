using Azure.Data.Tables;
using WispoRoboto.Chatbot.AzureTables.Entities;
using WispoRoboto.Chatbot.Extensions;
using WispoRoboto.Chatbot.Telegram.Contracts;
using WispoRoboto.Chatbot.Telegram.Models;
using WispoRoboto.Chatbot.Vectors.Contracts;

namespace WispoRoboto.Chatbot.AzureTables.Repositories;

public class AzureTablesPineconeMessageRepository : IMessageRepository
{
    private readonly IVectorFactory _vectorFactory;

    private readonly IVectorRepository _vectorRepository;

    private readonly TableClient _azureTableClient;

    public AzureTablesPineconeMessageRepository(IVectorFactory vectorFactory, IVectorRepository vectorRepository,
        TableClient azureTableClient)
    {
        _vectorFactory = vectorFactory;
        _vectorRepository = vectorRepository;
        _azureTableClient = azureTableClient;
    }

    public async Task Upsert(Message message, CancellationToken cancellationToken)
    {
        var tableClientMessage = new TelegramMessageAzureTables()
        {
            ChatTitle = message.Chat.Title,
            Date = message.Date.UnixTimestampToDateTime(),
            From = $"{message.From.FirstName} {message.From.LastName}",
            Text = message.Text,
            MessageId = message.MessageId.ToString(),
            PartitionKey = message.Chat.Title,
            RowKey = message.MessageId.ToString(),
            Timestamp = message.Date.UnixTimestampToDateTime()
        };
        await _azureTableClient.UpsertEntityAsync(tableClientMessage, cancellationToken: cancellationToken);
    }

    public async Task IndexAsChunkedEmbedding(Message message, CancellationToken cancellationToken)
    {
        var pages = _azureTableClient
            .QueryAsync<TelegramMessageAzureTables>(m => m.PartitionKey == message.Chat.Title, 100)
            .AsPages();
        var data = new List<TelegramMessageAzureTables>();
        await foreach (var page in pages)
        {
            data.AddRange(page.Values);
        }

        if (data.Count >= 5 || DateTime.UtcNow - GetMostRecentEntry(data).Date > TimeSpan.FromHours(6))
        {
            await ConcatMessagesAndSendToPinecone(data);
            foreach (var telegramMessageAzureTables in data)
            {
                await _azureTableClient.DeleteEntityAsync(telegramMessageAzureTables.PartitionKey,
                    telegramMessageAzureTables.RowKey, cancellationToken: cancellationToken);
            }
        }
    }

    private TelegramMessageAzureTables GetMostRecentEntry(List<TelegramMessageAzureTables> entries)
    {
        entries.Sort((x, y) => DateTimeOffset.Compare(x.Date, y.Date));
        return entries.Last();
    }

    private async Task ConcatMessagesAndSendToPinecone(List<TelegramMessageAzureTables> entries)
    {
        var data = $"""
                    ChatInteraction:
                    StartDate: {entries.First().Date:s}
                    {string.Join("\n", entries.Select(FormatMessageEntry))}
                    """;
        var vector = await _vectorFactory.Create(entries.First().MessageId, data, new Dictionary<string, string>()
        {
            { "text", data }
        });
        await _vectorRepository.Upsert(new[] { vector });
    }

    private string FormatMessageEntry(TelegramMessageAzureTables message)
    {
        return $"""
                from: {message.From}
                message: {message.Text}
                """;
    }
}