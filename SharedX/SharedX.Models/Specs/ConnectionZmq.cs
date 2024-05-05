namespace SharedX.Core.Specs;
public  class ConnectionZmq
{
    public PushPull MatchingToDropCopy { get; set; } = null!;
    public PushPull MatchingToMarketData { get; set; } = null!;
    public PushPull MatchingToOrderEngine { get; set; } = null!;
    public PushPull OrderEntryToOrderEngine { get; set; } = null!;
    public PushPull OrderEngineToMatching { get; set; } = null!;
}
public class PushPull
{
    public string Uri { get; set; }=string.Empty;
}