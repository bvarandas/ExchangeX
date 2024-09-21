using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core.Account;
using SharedX.Core.Enums;
using SharedX.Core.Extensions;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Specs;
using SharedX.Core.ValueObjects;
using System.Collections.Concurrent;
namespace TestEngineX;
public class ProducerOrderApp : BackgroundService
{
    private readonly ILogger<ProducerOrderApp> _logger;
    private PushSocket _sender;
    private readonly ConnectionZmq _config;
    private static Thread ThreadSenderOrder = null!;
    private static Thread ThreadSendBuyQueueOrder = null!;
    private static Thread ThreadSendSellQueueOrder = null!;
    private readonly IOutboxCache<OrderEngine> _cache;

    private readonly ConcurrentQueue<OrderEngine> OrderQueue;

    public ProducerOrderApp(ILogger<ProducerOrderApp> logger,
        IOptions<ConnectionZmq> options,
        IOutboxCache<OrderEngine> cache)
    {
        _logger = logger;
        _config = options.Value;
        _cache = cache;
        OrderQueue = new ConcurrentQueue<OrderEngine>();
    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Sender Order ZeroMQ...");

        ThreadSenderOrder = new Thread(() => SenderOrder(cancellationToken));
        ThreadSenderOrder.Name = nameof(ThreadSenderOrder);
        ThreadSenderOrder.Start();

        //ThreadSendBuyQueueOrder = new Thread(() => SendBuyOrder(cancellationToken));
        //ThreadSendBuyQueueOrder.Name = nameof(ThreadSendBuyQueueOrder);
        //ThreadSendBuyQueueOrder.Start();

        ThreadSendSellQueueOrder = new Thread(() => SendSellOrder(cancellationToken));
        ThreadSendSellQueueOrder.Name = nameof(ThreadSendSellQueueOrder);
        ThreadSendSellQueueOrder.Start();

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
                            var envelopeOrder = new EnvelopeOutbox<OrderEngine>();
                            envelopeOrder.Body = order;
                            envelopeOrder.Id = order.OrderID;
                            envelopeOrder.LastTransaction = DateTime.UtcNow;
                            envelopeOrder.ActivityOutbox = new ActivityOutbox() { Activity = "OrderEntry" };

                            _cache.TryEnqueueZeroMQEnvelope(envelopeOrder);
                            _cache.TryEnqueueRabitMQEnvelope(envelopeOrder);
                            _cache.UpsertOutboxAsync(envelopeOrder);

                            var message = envelopeOrder.SerializeToByteArrayProtobuf<EnvelopeOutbox<OrderEngine>>();
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

    private void SendBuyOrder(CancellationToken stoppingToken)
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


    private void SendSellOrder(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            var order = new OrderEngine();
            order.Symbol = "btcusd";
            order.Side = SideTrade.Sell;
            order.Account = new Limit() { AccountId = 10013, };
            order.TimeInForce = TimeInForce.FOK;
            order.AccountId = 10013;
            order.Execution = Execution.ToOpen;
            order.MinQty = 0;
            order.Quantity = 0.5M;
            order.Price = 65803M;
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
