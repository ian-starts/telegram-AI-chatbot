using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using WispoRoboto.Chatbot.LLM.Contracts;

namespace WispoRoboto.Chatbot.LLM;

public class ChatGpt4Handler : ILLMQueryable
{
    private readonly IOpenAIAPI _openAiapi;

    public ChatGpt4Handler(IOpenAIAPI openAiapi)
    {
        _openAiapi = openAiapi;
    }

    public async Task<string> GetQueryResponse(string query, string context)
    {
        // Incorporate all pinecone matches in the system prompt
        var systemPrompt =
            $"""
             You're a helpful assistent, who has access to the following context.
             The context is a set of messages from a chatgroup called Hummushomies that best match the query.
             Context:
             {context}
             """;

        var resp = await _openAiapi.Chat.CreateChatCompletionAsync(new List<ChatMessage>()
        {
            new(ChatMessageRole.System, systemPrompt),
            new(ChatMessageRole.User, query)
        }, model: "gpt-4-1106-preview");

        return resp.Choices[0].Message.Content;
    }
}