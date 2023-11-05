namespace WispoRoboto.Chatbot.LLM.Contracts;

public interface ILLMQueryable
{
    public Task<string> GetQueryResponse(string query, string context);
}