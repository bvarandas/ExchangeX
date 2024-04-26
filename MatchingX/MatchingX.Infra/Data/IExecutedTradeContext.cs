using MongoDB.Driver;
using SharedX.Core.Matching;

namespace MatchingX.Infra.Data;
public interface IExecutedTradeContext
{
    IMongoCollection<ExecutedTrade> ExecutedTrade { get; }
}