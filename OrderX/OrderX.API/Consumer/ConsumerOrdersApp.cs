using MassTransit;
using OrderEngineX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;

namespace OrderX.API.Consumer;
public class ConsumerOrdersApp : IConsumer<OrderEngine>
{
    private readonly ILogger<ConsumerOrdersApp> _logger;
    private readonly IOrderEngineCache _cache;
    public ConsumerOrdersApp(ILogger<ConsumerOrdersApp> logger, IOrderEngineCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public Task Consume(ConsumeContext<OrderEngine> context)
    {
        Console.Write(context.Message);
        _cache.AddOrder(context.Message);
        return Task.CompletedTask;
    }
}