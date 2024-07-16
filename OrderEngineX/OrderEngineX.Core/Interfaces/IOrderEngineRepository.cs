using FluentResults;
using SharedX.Core.Matching.OrderEngine;
namespace OrderEngineX.Core.Interfaces;
public interface IOrderEngineRepository
{
    Task<Result> UpsertOrdersAsync(OrderEngine order, CancellationToken cancellationToken);
    Task<Result<IEnumerable<OrderEngine>>> GetOrdersOnRestartAsync(CancellationToken cancellation);
    Task<Result<IEnumerable<OrderEngine>>> GetOrdersByAccountIdAsync(int accountId, CancellationToken cancellation);
    Task<Result<OrderEngine>> GetOrderByIdAsync(long orderId, CancellationToken cancellation);
    Task<Result> CreateOrdersAsync(OrderEngine order, CancellationToken cancellation);
    Task<Result> UpdateOrderDetailAsync(OrderEngine order, OrderEngineDetail oderDetail, CancellationToken cancellation);
    Task<Result<long>> GetOrderIdAsync(CancellationToken cancellation);
}
