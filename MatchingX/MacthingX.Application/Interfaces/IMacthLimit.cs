using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Interfaces;
public interface IMatchLimit
{
    void ReceiveOrder(OrderEngine order);
}
