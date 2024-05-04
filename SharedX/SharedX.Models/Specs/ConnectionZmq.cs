namespace SharedX.Core.Specs;
public  class ConnectionZmq
{
    public PubSub Consumer { get; set; } = null!;
    public PubSub ConsumerExecutionReport { get; set; } = null!;
    public PubSub PublisherDropCopy { get; set; } = null!;
    public PubSub PublisherMarketData { get; set; } = null!;
    public PubSub PublisherOrders { get; set; } = null!;
}
public class PubSub
{
    public string Uri { get; set; }=string.Empty;
    public IList<string> Topics {  get; set; }=null!;
}