using SharedX.Core.Matching;
namespace ShareX.Core.Interfaces;
public interface IOrderBook
{
    void AddOrder(Book book);
    void ReplaceOrder(Book book);
    void CancelOrder(Book book);
}