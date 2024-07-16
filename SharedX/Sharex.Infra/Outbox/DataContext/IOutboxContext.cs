using MongoDB.Driver;
using SharedX.Core.ValueObjects;

namespace Sharedx.Infra.Outbox.Data;
public interface IOutboxContext<T> where T : class
{
    IMongoCollection<EnvelopeOutbox<T>> Collection { get; }
    MongoClient MongoClient { get; }
}