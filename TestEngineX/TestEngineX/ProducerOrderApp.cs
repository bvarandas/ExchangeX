using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core.Account;
using SharedX.Core.Enums;
using SharedX.Core.Extensions;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Specs;
using System.Collections.Concurrent;
namespace TestEngineX;
public class ProducerOrderApp : BackgroundService
{
    private readonly ILogger<ProducerOrderApp> _logger;
    private PushSocket _sender;
    private readonly ConnectionZmq _config;
    private static Thread ThreadSenderOrder = null!;
    private static Thread ThreadFillQueueOrder = null!;

    private readonly ConcurrentQueue<OrderEngine> OrderQueue;

    public ProducerOrderApp(ILogger<ProducerOrderApp> logger,
        IOptions<ConnectionZmq> options)
    {
        _logger = logger;
        _config = options.Value;
        OrderQueue = new ConcurrentQueue<OrderEngine>();
    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Sender Order ZeroMQ...");

        ThreadSenderOrder = new Thread(() => SenderOrder(cancellationToken));
        ThreadSenderOrder.Name = nameof(ThreadSenderOrder);
        ThreadSenderOrder.Start();

        ThreadFillQueueOrder = new Thread(() => FillOrder(cancellationToken));
        ThreadFillQueueOrder.Name = nameof(ThreadFillQueueOrder);
        ThreadFillQueueOrder.Start();

        return Task.CompletedTask;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }


    private void SenderOrder(CancellationToken stoppingToken)
    {
        bool isConnected = false;
        do
        {
            try
            {
                _logger.LogInformation($"Sender de ordens tentando conectar..{_config.PublisherEngine.Uri}");
                using (_sender = new PushSocket("@" + _config.PublisherEngine.Uri))
                //using (_sender = new PushSocket(""))
                {
                    _logger.LogInformation("Sender de ordens Conectado!!!");
                    isConnected = true;

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        while (OrderQueue.TryDequeue(out OrderEngine order))
                        {
                            var message = order.SerializeToByteArrayProtobuf<OrderEngine>();
                            _sender.SendMultipartBytes(message);
                        }
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                isConnected = false;
                _logger.LogError(ex.Message, ex);
            }

            Thread.Sleep(100);
        } while (!isConnected);
    }

    private void FillOrder(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var order = new OrderEngine();
            order.Symbol = "btcusd";
            order.Side = SideTrade.Buy;
            order.Account = new Limit() { AccountId = 10012, };
            order.TimeInForce = TimeInForce.FOK;
            order.AccountId = 10012;
            order.Execution = Execution.ToOpen;
            order.MinQty = 0;
            order.Quantity = 0.5M;
            order.Price = 65802M;
            order.LastPrice = 0;
            order.ClOrdID = 123;
            order.LastQuantity = order.Quantity;
            order.OrderStatus = OrderStatus.New;
            order.OrderType = OrderType.Limit;
            order.ParticipatorId = 10;
            order.StopPrice = 0;
            order.OrigClOrdID = 0;

            OrderQueue.Enqueue(order);

            Thread.Sleep(1000);
        }
    }
}
