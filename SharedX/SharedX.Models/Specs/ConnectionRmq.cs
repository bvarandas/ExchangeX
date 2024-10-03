namespace SharedX.Core.Specs;
public class ConnectionRmq
{
    public PushPull ReceiverEngine { get; set; } = null!;
    public PushPull PublisherEngine { get; set; } = null!;

}
