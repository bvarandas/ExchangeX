using SharedX.Core.Matching.OrderEngine;
namespace ShareX.Core.Interfaces;
public interface IOrderBook
{
    void AddOrder(OrderEngine order);
    void ReplaceOrder(OrderEngine order);
    void CancelOrder(OrderEngine orderToCancel);
}