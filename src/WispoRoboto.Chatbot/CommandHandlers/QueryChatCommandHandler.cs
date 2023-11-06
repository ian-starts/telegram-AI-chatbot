using Pinecone;
using Telegram.Bot;
using WispoRoboto.Chatbot.CommandHandlers.Contracts;
using WispoRoboto.Chatbot.LLM.Contracts;
using WispoRoboto.Chatbot.Telegram.Models;
using WispoRoboto.Chatbot.Vectors.Contracts;

namespace WispoRoboto.Chatbot.CommandHandlers;

public class QueryChatCommandHandler : ICommandHandler
{
    private readonly IVectorFactory _vectorFactory;

    private readonly IVectorRepository _vectorRepository;

    private readonly ILLMQueryable _llmQueryable;

    private readonly ITelegramBotClient _telegramBotClient;

    public QueryChatCommandHandler(IVectorFactory vectorFactory, IVectorRepository vectorRepository,
        ILLMQueryable llmQueryable, ITelegramBotClient telegramBotClient)
    {
        _vectorFactory = vectorFactory;
        _vectorRepository = vectorRepository;
        _llmQueryable = llmQueryable;
        _telegramBotClient = telegramBotClient;
    }

    public bool CanHandle(TelegramRequest request)
    {
        return request.Message is { IsCommand: true, Command: "/query" } message &&
               !string.IsNullOrEmpty(message.Argument);
    }

    public async Task HandleMessage(TelegramRequest request)
    {
        var vector = await _vectorFactory.Create(request.Message.MessageId.ToString(), request.Message.Text,
            new Dictionary<string, string>());

        var bestMatches = await _vectorRepository.Query(vector);

        var llmResponse =
            await _llmQueryable.GetQueryResponse(request.Message.Argument, GetContextFromMatches(bestMatches));

        await _telegramBotClient.SendTextMessageAsync(chatId: request.Message.Chat.Id, text: llmResponse);
    }

    private string GetContextFromMatches(ScoredVector[] scoredVectors)
    {
        return string.Join("\n",
            scoredVectors.Select(v => v.Metadata?.First(m => m.Key == "text").Value.Inner?.ToString())
                .OfType<string>());
    }
}