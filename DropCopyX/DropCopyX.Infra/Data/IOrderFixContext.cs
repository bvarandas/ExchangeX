using MongoDB.Driver;
using SharedX.Core.Matching;

namespace DropCopyX.Infra.Data;
public interface IOrderFixContext
{
    IMongoCollection<Order> OrderTrade { get; }
}
