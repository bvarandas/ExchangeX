using FluentResults;
using MatchingX.Core.Entities;
using SharedX.Core.Enums;
namespace MatchingX.Core.Interfaces;

public interface IOrderStopCache
{
    Task<bool> DeleteOrderAsync(string symbol, long orderId, SideTrade side);
    Task UpsertOrderAsync(MatchOrder order);
    Task<Result<MatchOrder>> GetOrderByIdandSymbolAsync(long orderId, string symbol, SideTrade side);
    Task<Result<Dictionary<long, MatchOrder>>> GetOrderBySymbolAsync(string symbol, SideTrade side);
}