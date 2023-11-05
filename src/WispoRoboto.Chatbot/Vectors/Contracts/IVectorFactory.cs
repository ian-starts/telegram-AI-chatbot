using Pinecone;

namespace WispoRoboto.Chatbot.Vectors.Contracts;

public interface IVectorFactory
{
    public Task<Vector> Create(string id, string dataToBaseVectorOn, Dictionary<string, string> metaData);
}