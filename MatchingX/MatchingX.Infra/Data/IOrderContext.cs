using MongoDB.Driver;
using SharedX.Core.Matching;

namespace MatchingX.Infra.Data;
public interface IOrderContext
{
    IMongoCollection<Order> OrderTrade { get; }
}
