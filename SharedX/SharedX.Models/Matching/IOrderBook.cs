using SharedX.Core.Matching;
namespace ShareX.Core.Interfaces;
public interface IOrderBook
{
    void AddOrder(Order order);
    void ReplaceOrder(Order order);
    void CancelOrder(Order orderToCancel);
}