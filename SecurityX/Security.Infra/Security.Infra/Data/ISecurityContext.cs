using MongoDB.Driver;
using SharedX.Core.Entities;
namespace Security.Infra.Data;
public interface ISecurityContext
{
    IMongoCollection<SecurityEngine> SecurityEngine { get; }
    MongoClient MongoClient { get; }
}
