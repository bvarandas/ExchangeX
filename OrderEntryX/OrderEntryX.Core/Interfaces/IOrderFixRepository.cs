using OrderEntryX.Core.Filters;
using SharedX.Core.Matching;
namespace OrderEntryX.Core.Interfaces;
public interface IOrderFixRepository
{
    Task<IEnumerable<OrderEng>> GetOrdersAsync(OrderFixParams specParams, CancellationToken cancellation);
    Task<bool> CreateOrdersAsync(OrderEng order, CancellationToken cancellation);
    Task<bool> UpdateOrderAsync(OrderEng order, CancellationToken cancellation);
}
