using MongoDB.Driver;
using SharedX.Core.Matching.DropCopy;
namespace MatchingX.Infra.Data;
public interface IExecutedTradeContext
{
    IMongoCollection<TradeReport> ExecutedTrade { get; }
}