using MacthingX.Application.Services;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Interfaces;
public interface ITradeOrderService
{
    public event PriceChangedEventHandler PriceChanged;
    bool AddOrder(OrderEngine order);
    bool CancelOrder(OrderEngine order);
    Task<bool> ModifyOrder(OrderEngine order);
    Task<bool> RemoveCancelledOrdersAsync(OrderEngine order);
    Task<bool> RemoveTradedOrdersAsync(Dictionary<long,OrderEngine> dicOrders);
    void CreateReports(OrderEngine order, Dictionary<long, OrderEngine> dicOrders);
}