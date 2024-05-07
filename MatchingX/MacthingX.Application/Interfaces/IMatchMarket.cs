using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Interfaces;
public interface IMatchMarket
{
    void ReceiveOrder(OrderEngine order);
}
