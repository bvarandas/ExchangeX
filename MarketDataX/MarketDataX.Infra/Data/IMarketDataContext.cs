using MarketDataX.Core.Entities;
using MongoDB.Driver;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Matching.MarketData;

namespace MarketDataX.Infra.Data;
public interface IMarketDataContext
{
    IMongoCollection<MarketData> MarketData { get; }

    IMongoCollection<Login> Login { get; }
}