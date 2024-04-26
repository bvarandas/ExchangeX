namespace SharedX.Core.Specs;
public abstract class QueueSettings
{
    public string HostName { get; set; } = string.Empty;
    public string QueueNameOrderBook { get; set; } = string.Empty;
    public string ExchangeService { get; set; } = string.Empty;
    public string ExchangeType { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public int Port { get; set; }
    public ushort Interval { get; set; }
}

public class QueueCommandSettings : QueueSettings { }
public class QueueEventSettings : QueueSettings { }
