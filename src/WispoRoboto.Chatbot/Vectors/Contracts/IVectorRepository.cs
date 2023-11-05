using Pinecone;

namespace WispoRoboto.Chatbot.Vectors.Contracts;

public interface IVectorRepository
{
    public Task Upsert(IEnumerable<Vector> vector, CancellationToken cancellationToken = default);

    public Task<ScoredVector[]> Query(Vector vector,
        uint topValues = 20,
        CancellationToken cancellationToken = default);
}