using OrderEntryX.Core.Filters;
using SharedX.Core.Matching;
namespace OrderEntryX.Core.Interfaces;
public interface IOrderFixRepository
{
    Task<IEnumerable<Order>> GetOrdersAsync(OrderFixParams specParams, CancellationToken cancellation);
    Task<bool> CreateOrdersAsync(Order order, CancellationToken cancellation);
    Task<bool> UpdateOrderAsync(Order order, CancellationToken cancellation);
}
