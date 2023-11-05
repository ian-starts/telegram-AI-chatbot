using WispoRoboto.Chatbot.Telegram.Models;

namespace WispoRoboto.Chatbot.CommandHandlers.Contracts;

public interface ICommandHandler
{
    public bool CanHandle(TelegramRequest request);

    public Task HandleMessage(TelegramRequest request);
}