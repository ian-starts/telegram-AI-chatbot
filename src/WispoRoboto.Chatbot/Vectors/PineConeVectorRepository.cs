using Pinecone;
using Pinecone.Grpc;
using WispoRoboto.Chatbot.Vectors.Contracts;

namespace WispoRoboto.Chatbot.Vectors;

public class PineConeVectorRepository : IVectorRepository
{
    private readonly Index<GrpcTransport> _pineConeIndex;

    public PineConeVectorRepository(Index<GrpcTransport> pineConeIndex)
    {
        _pineConeIndex = pineConeIndex;
    }

    public async Task Upsert(IEnumerable<Vector> vector, CancellationToken cancellationToken = default)
    {
        await _pineConeIndex.Upsert(vector);
    }

    public async Task<ScoredVector[]> Query(Vector vector, uint topValues = 20, CancellationToken cancellationToken = default)
    {
        return await _pineConeIndex.Query(vector.Values, topValues, includeMetadata: true);
    }
}