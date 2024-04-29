using MongoDB.Driver;
using SharedX.Core.Proto;

namespace MatchingX.Infra.Data;
public interface IExecutedTradeContext
{
    IMongoCollection<ExecutedTrade> ExecutedTrade { get; }
}