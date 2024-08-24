namespace SharedX.Core.Specs;
public  class ConnectionZmq
{
    //public PushPull MatchingToDropCopy { get; set; } = null!;
    //public PushPull MatchingToMarketData { get; set; } = null!;
    //public PushPull MatchingToOrderEngine { get; set; } = null!;
    //public PushPull MatchingToTradeEngine { get; set; } = null!;
    //public PushPull OrderEntryToOrderEngine { get; set; } = null!;
    //public PushPull OrderEngineToMatching { get; set; } = null!;
    //public PushPull SecurityToMatching { get; set; } = null!;
    //public PushPull SecurityToMarketData { get; set; } = null!;
    //public PushPull SecurityToOrderEngine { get; set; } = null!;

    public PushPull ReceiverEngine { get; set; } = null!;
    public PushPull PublisherEngine { get; set; } = null!;

}
public class PushPull
{
    public string Uri { get; set; }=string.Empty;
}

public class ConnectionZeroMq
{
    public PushPull PushPullAddress { get; set; } = null!;
}