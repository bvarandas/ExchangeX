using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Interfaces;
public interface IMatchStop
{
    void ReceiveOrder(OrderEngine order);
}