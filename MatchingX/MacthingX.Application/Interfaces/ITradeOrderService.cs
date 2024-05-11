using MacthingX.Application.Services;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Interfaces;
public interface ITradeOrderService
{
    public event PriceChangedEventHandler PriceChanged;
    bool AddOrder(OrderEngine order);
    bool CancelOrder(OrderEngine orderToCancel);
    bool ReplaceOrder(OrderEngine orderToReplace);
    Task<bool> RemoveCancelledOrdersAsync(OrderEngine orderToCancel);
    Task<bool> RemoveTradedOrdersAsync(OrderEngine buyOrder, OrderEngine sellOrder);
    TradeCaptureReport CreateTradeCaptureCancelled(OrderEngine order);
    (TradeCaptureReport, TradeCaptureReport) CreateTradeCapture(OrderEngine orderBuyer, OrderEngine orderSeller);
}