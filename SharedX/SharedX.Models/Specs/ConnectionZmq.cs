namespace SharedX.Core.Specs;

public  class ConnectionZmq
{
    public PubSub Consumer { get; set; } = null!;
    public PubSub Publisher { get; set; } = null!;
}

public class PubSub
{
    public string Uri { get; set; }=string.Empty;
    public string Topic {  get; set; }=string.Empty;

}
