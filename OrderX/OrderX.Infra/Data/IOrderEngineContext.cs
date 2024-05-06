using MongoDB.Driver;
using SharedX.Core.Matching.OrderEngine;
namespace OrderEngineX.Infra.Data;
public interface IOrderEngineContext
{
    IMongoCollection<OrderEngine> OrderEngine { get; }
}