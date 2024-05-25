using FluentResults;
using SharedX.Core.Matching.OrderEngine;
namespace OrderEngineX.Core.Interfaces;
public interface IOrderEngineCache
{
    void AddOrder(OrderEngine order);
    bool TryDequeueOrder(out OrderEngine    order);
    Task<Result<Dictionary<string, OrderEngine>>> GetOrdersBySymbolAsync(string symbol, DateTime date);
    Task<Result<Dictionary<string, OrderEngine>>> GetOrdersAsync(DateTime date);
}