using OrderEntryX.Core.Entities;

namespace OrderEntryX.Core.Repositories;
public interface IOrderEntryRepository
{
    Task<bool> CreateOrdersAsync(OrderEntry order, CancellationToken cancellationToken);
}
