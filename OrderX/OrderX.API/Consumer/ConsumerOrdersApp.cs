using MassTransit;
using SharedX.Core.Matching;

namespace OrderX.API.Consumer;
public class ConsumerOrdersApp : BackgroundService
{
    private readonly ILogger<ConsumerOrdersApp> _logger;
    public ConsumerOrdersApp(ILogger<ConsumerOrdersApp> logger)
    {
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }

    public async override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Finalizando o consumer RabbitMQ...");
        

        _logger.LogInformation("Consumer RabbitMq...Finalizado!");
    }
}

public class ConsumerOrders : IConsumer<Order>
{
    private readonly ILogger<ConsumerOrders> _logger;
    public ConsumerOrders(ILogger<ConsumerOrders> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<Order> context)
    {
        Console.Write(context.Message);

        return Task.CompletedTask;
    }
}