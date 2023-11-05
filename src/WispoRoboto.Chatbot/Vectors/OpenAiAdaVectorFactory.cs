using OpenAI_API;
using OpenAI_API.Embedding;
using OpenAI_API.Models;
using Pinecone;
using WispoRoboto.Chatbot.Vectors.Contracts;

namespace WispoRoboto.Chatbot.Vectors;

public class OpenAiAdaVectorFactory : IVectorFactory
{
    private readonly IOpenAIAPI _openAiApi;
    private readonly Model _openAiModel;

    public OpenAiAdaVectorFactory(IOpenAIAPI openAiApi, Model? openAiModel = null)
    {
        _openAiApi = openAiApi;
        _openAiModel = openAiModel ?? Model.AdaTextEmbedding;
    }

    public async Task<Vector> Create(string id, string dataToBaseVectorOn, Dictionary<string, string> metaData)
    {
        var embedding = await _openAiApi.Embeddings.CreateEmbeddingAsync(new EmbeddingRequest
        {
            Model = _openAiModel,
            Input = dataToBaseVectorOn
        });
        var metadataMap = new MetadataMap();
        foreach (var (key, value) in metaData)
        {
            metadataMap.Add(key, value);
        }

        return new Vector
        {
            Id = id,
            Values = embedding.Data.First().Embedding,
            Metadata = metadataMap
        };
    }
}