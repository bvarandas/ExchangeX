using SharedX.Core.Matching;
namespace ShareX.Core.Interfaces;
public interface IOrderBook
{
    void AddOrder(OrderEng order);
    void ReplaceOrder(OrderEng order);
    void CancelOrder(OrderEng orderToCancel);
}