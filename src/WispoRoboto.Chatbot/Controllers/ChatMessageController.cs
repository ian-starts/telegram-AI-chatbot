using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using WispoRoboto.Chatbot.CommandHandlers.Contracts;
using WispoRoboto.Chatbot.Extensions;
using WispoRoboto.Chatbot.Telegram.Models;
using WispoRoboto.Chatbot.Vectors.Contracts;

namespace WispoRoboto.Chatbot.Controllers;

public class ChatMessageController
{
    private readonly ILogger<ChatMessageController> _logger;

    private readonly IVectorFactory _vectorFactory;

    private readonly IVectorRepository _vectorRepository;

    private readonly IEnumerable<ICommandHandler> _commandHandlers;

    public ChatMessageController(ILogger<ChatMessageController> logger, IVectorFactory vectorFactory,
        IVectorRepository vectorRepository, IEnumerable<ICommandHandler> commandHandlers)
    {
        _logger = logger;
        _vectorFactory = vectorFactory;
        _vectorRepository = vectorRepository;
        _commandHandlers = commandHandlers;
    }

    [Function("ChatMessage")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")]
        HttpRequestData req,
        FunctionContext executionContext)
    {
        try
        {
            var telegramMessage = await JsonSerializer.DeserializeAsync<TelegramRequest>(req.Body);
            if (telegramMessage == null)
            {
                return req.CreateResponse(HttpStatusCode.Accepted);
            }

            if (telegramMessage.Message.IsCommand)
            {
                await HandleCommand(telegramMessage);
            }
            else
            {
                await StoreMessageInPineCone(telegramMessage);
            }

            var response = req.CreateResponse(HttpStatusCode.Accepted);

            return response;
        }
        catch (Exception)
        {
            _logger.LogInformation("Not a chat message so skipping");
            return req.CreateResponse(HttpStatusCode.Accepted);
        }
    }

    private async Task HandleCommand(TelegramRequest telegramMessage)
    {
        var handler = _commandHandlers.FirstOrDefault(h => h.CanHandle(telegramMessage));
        if (handler == null)
        {
            return;
        }

        await handler.HandleMessage(telegramMessage);
    }

    private async Task StoreMessageInPineCone(TelegramRequest telegramMessage)
    {
        var vectorMessage = FormatAsVectorMessage(telegramMessage);
        var vector = await _vectorFactory.Create(telegramMessage.Message.MessageId.ToString(),
            vectorMessage,
            new Dictionary<string, string>
            {
                { "text", vectorMessage },
                { "from", $"{telegramMessage.Message.From.FirstName} {telegramMessage.Message.From.LastName}" },
                { "chatName", telegramMessage.Message.Chat.Title }
            });

        await _vectorRepository.Upsert(new[] { vector });
    }

    private string FormatAsVectorMessage(TelegramRequest request)
    {
        return
            $"""
             ChatMessage
             from: {request.Message.From.FirstName} {request.Message.From.LastName}
             chat title: {request.Message.Chat.Title}
             date: {request.Message.Date.UnixTimestampToDateTime():s}
             message: {request.Message.Text}
             """;
    }
}