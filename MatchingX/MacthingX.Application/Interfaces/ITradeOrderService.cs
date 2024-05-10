using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Interfaces;
public interface ITradeOrderService
{
    void AddOrder(OrderEngine order);
    void CancelOrder(OrderEngine orderToCancel);
    void ReplaceOrder(OrderEngine orderToReplace);
    Task<bool> RemoveCancelledOrdersAsync(OrderEngine orderToCancel);
    Task<bool> RemoveTradedOrdersAsync(OrderEngine buyOrder, OrderEngine sellOrder);
    TradeCaptureReport CreateTradeCaptureCancelled(OrderEngine order);
    (TradeCaptureReport, TradeCaptureReport) CreateTradeCapture(OrderEngine orderBuyer, OrderEngine orderSeller);
}