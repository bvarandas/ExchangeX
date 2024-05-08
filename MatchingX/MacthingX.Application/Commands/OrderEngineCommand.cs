using SharedX.Core.Commands;
using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Commands;
public abstract class OrderEngineCommand : Command
{
    public OrderEngine Order { get; protected set; } = new OrderEngine();
    public DateTime Timestamp { get; protected set; } = DateTime.Now;
}