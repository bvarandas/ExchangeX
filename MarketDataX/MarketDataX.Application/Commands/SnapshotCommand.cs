using SharedX.Core.Commands;
using SharedX.Core.Matching.MarketData;
namespace MarketDataX.Application.Commands;
public class SnapshotCommand : Command
{
    public readonly MarketDataSnapshot Snapshot;
    public DateTime Timestamp { get; private set; }
    public SnapshotCommand(MarketDataSnapshot snapshot)
    {
        Timestamp = DateTime.Now;
        Snapshot = snapshot;
    }
    public override bool IsValid()
    {
        throw new NotImplementedException();
    }
}