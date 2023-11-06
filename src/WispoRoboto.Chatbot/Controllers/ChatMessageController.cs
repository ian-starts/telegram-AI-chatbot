using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using WispoRoboto.Chatbot.CommandHandlers.Contracts;
using WispoRoboto.Chatbot.Extensions;
using WispoRoboto.Chatbot.Telegram.Contracts;
using WispoRoboto.Chatbot.Telegram.Models;
using WispoRoboto.Chatbot.Vectors.Contracts;

namespace WispoRoboto.Chatbot.Controllers;

public class ChatMessageController
{
    private readonly ILogger<ChatMessageController> _logger;

    private readonly IVectorFactory _vectorFactory;

    private readonly IVectorRepository _vectorRepository;

    private readonly IMessageRepository _messageRepository;

    private readonly IEnumerable<ICommandHandler> _commandHandlers;

    public ChatMessageController(ILogger<ChatMessageController> logger, IVectorFactory vectorFactory,
        IVectorRepository vectorRepository, IEnumerable<ICommandHandler> commandHandlers,
        IMessageRepository messageRepository)
    {
        _logger = logger;
        _vectorFactory = vectorFactory;
        _vectorRepository = vectorRepository;
        _commandHandlers = commandHandlers;
        _messageRepository = messageRepository;
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
        await _messageRepository.Upsert(telegramMessage.Message);
        await _messageRepository.IndexAsChunkedEmbedding(telegramMessage.Message);
    }
}