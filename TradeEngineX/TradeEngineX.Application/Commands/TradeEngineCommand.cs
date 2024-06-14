using SharedX.Core.Commands;
using SharedX.Core.Matching.DropCopy;
namespace TradeEngineX.Application.Commands;
public abstract class TradeEngineCommand : Command
{
    public CancellationToken CancellationToken { get; set; }
    public TradeReport TradeReport { get; protected set; } = new TradeReport();
    public DateTime Timestamp { get; protected set; } = DateTime.Now;
}