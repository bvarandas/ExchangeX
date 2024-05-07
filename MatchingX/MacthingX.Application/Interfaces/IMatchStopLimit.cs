using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Interfaces;
public  interface IMatchStopLimit
{
    void ReceiveOrder(OrderEngine order);
}