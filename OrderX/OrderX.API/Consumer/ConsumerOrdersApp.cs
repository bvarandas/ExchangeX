using MassTransit;
using SharedX.Core.Matching;
using OrderEngineX.Core.Interfaces;
namespace OrderX.API.Consumer;
public class ConsumerOrdersApp : IConsumer<OrderEng>
{
    private readonly ILogger<ConsumerOrdersApp> _logger;
    private readonly IOrderEngineCache _cache;
    public ConsumerOrdersApp(ILogger<ConsumerOrdersApp> logger, IOrderEngineCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public Task Consume(ConsumeContext<OrderEng> context)
    {
        Console.Write(context.Message);
        _cache.AddOrder(context.Message);
        return Task.CompletedTask;
    }
}