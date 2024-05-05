using SharedX.Core.Matching;
namespace MacthingX.Application.Interfaces;
public interface IMatchStop
{
    void ReceiveOrder(Order order);
}