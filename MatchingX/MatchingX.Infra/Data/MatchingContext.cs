using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SharedX.Core.Matching;
using SharedX.Core.Matching.OrderEngine;
namespace MatchingX.Infra.Data;

public class MatchingContext : IMatchingContext
{
    public IMongoCollection<OrderEngine> Matching { get; }
    public MongoClient MongoClient { get; }
    public MatchingContext(IConfiguration configuration)
    {
        MongoClient = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
        var database = MongoClient.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseNameMatching"));

        Matching = database.GetCollection<OrderEngine>(
            configuration.GetValue<string>("DatabaseSettings:CollectionMatching"));
    }
}
