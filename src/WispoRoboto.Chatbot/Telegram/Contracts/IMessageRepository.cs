using WispoRoboto.Chatbot.Telegram.Models;

namespace WispoRoboto.Chatbot.Telegram.Contracts;

public interface IMessageRepository
{
    public Task Upsert(Message message, CancellationToken cancellationToken = default);

    public Task IndexAsChunkedEmbedding(Message message, CancellationToken cancellationToken = default);
}