using FluentResults;
using SharedX.Core.Matching.OrderEngine;
namespace SharedX.Core.Interfaces;
public interface IOrderStopCache
{
    Task<bool> DeleteOrderAsync(string symbol, long orderId);
    void UpsertOrderAsync(OrderEngine order);
    Task<Result<OrderEngine>> GetOrderByIdandSymbolAsync(long orderId, string symbol);
    Task<Result<Dictionary<long, OrderEngine>>> GetOrderBySymbolAsync(string symbol);
}