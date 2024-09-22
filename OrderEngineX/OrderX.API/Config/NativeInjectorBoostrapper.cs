using FluentResults;
using MassTransit;
using MediatR;
using MongoDB.Driver;
using OrderEngineX.API.Publisher;
using OrderEngineX.API.Receiver;
using OrderEngineX.Application.Commands;
using OrderEngineX.Application.Commands.Order;
using OrderEngineX.Application.Events;
using OrderEngineX.Core.Interfaces;
using OrderEngineX.Core.Notifications;
using OrderEngineX.Infra.Cache;
using OrderEngineX.Infra.Data;
using OrderEngineX.Infra.Repositories;
using Sharedx.Infra.Order.Cache;
using Sharedx.Infra.Outbox.Cache;
using Sharedx.Infra.Outbox.Services;
using SharedX.Core.Bus;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Specs;
using SharedX.Core.ValueObjects;
using SharedX.Infra.Cache;
using System.Reflection;

namespace OrderEngineX.API.Config;
internal class NativeInjectorBoostrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration config)
    {
        services.AddSwaggerGen();

        services.Configure<QueueSettings>(config.GetSection(nameof(QueueSettings)));
        services.Configure<ConnectionZmq>(config.GetSection(nameof(ConnectionZmq)));
        services.Configure<ConnectionRmq>(config.GetSection(nameof(ConnectionRmq)));
        services.Configure<ConnectionRedis>(config.GetSection(nameof(ConnectionRedis)));

        services.AddMassTransit(x =>
        {
            x.AddConsumer<OutboxConsumerService<OrderEngine>>();

            x.UsingRabbitMq((context, cfg) =>
            {
                string hostname = config["QueueSettings:Hostname"]!;
                string port = config["QueueSettings:port"]!;

                //cfg.Host(hostname, port, "/", h =>
                cfg.Host(new Uri("rabbitmq://" + hostname + ":" + port), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ConfigureEndpoints(context);

                cfg.Message<EnvelopeOutbox<OrderEngine>>(x =>
                {
                    x.SetEntityName("outbox-order-engine");
                });

                cfg.ReceiveEndpoint(config.GetSection("QueueSettings:QueueName").Value, e =>
                {
                    e.PrefetchCount = 10;
                    e.UseMessageRetry(r => r.Interval(2, 100));
                    e.ConfigureConsumer<OutboxConsumerService<OrderEngine>>(context);
                });

            });
        });

        // Services
        services.AddSingleton(typeof(IReceiverEngine<OrderEngine>), typeof(ReceiverOrder));

        // Domain Bus (Mediator)
        services.AddScoped<IMediatorHandler, InMemmoryBus>();
        //services.AddScoped<IOrderBook, OrderBook>();

        //SignalR
        services.AddSignalR();

        // Mediator
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());
        });

        services.AddCors(options => options.AddPolicy("CorsPolicy", builderc =>
        {
            builderc
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed((host) => true)
            .AllowCredentials();
        }));

        // Outbox
        services.AddSingleton(typeof(IOutboxCache<OrderEngine>), typeof(OutboxCache<OrderEngine>));
        //services.AddSingleton(typeof(IOutboxPublisherService<OrderEngine>), typeof(OutboxPublisherService<OrderEngine>));
        //services.AddSingleton(typeof(IOutboxConsumerService<EnvelopeOutbox<OrderEngine>>),
        //    typeof(OutboxConsumerService<EnvelopeOutbox<OrderEngine>>));

        // Services
        services.AddSingleton(typeof(IPublisherEngine<OrderEngine>), typeof(PublisherOrder));

        // Domain - Events
        services.AddSingleton<INotificationHandler<DomainNotification>, DomainNotificationHandler>();

        services.AddSingleton<INotificationHandler<OrderEngineNewEvent>, OrderEngineEventHandler>();
        services.AddSingleton<INotificationHandler<OrderTradeModifyEvent>, OrderEngineEventHandler>();
        services.AddSingleton<INotificationHandler<OrderEngineCancelEvent>, OrderEngineEventHandler>();

        // Domain - Command
        services.AddSingleton<IRequestHandler<OrderCancelCommand, Result>, OrderEngineCommandHandler>();
        services.AddSingleton<IRequestHandler<OrderCancelReplaceCommand, Result>, OrderEngineCommandHandler>();
        services.AddSingleton<IRequestHandler<OrderOpenedCommand, Result>, OrderEngineCommandHandler>();

        // Infra - Data
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = new MongoClient(config.GetValue<string>("DatabaseSettings:ConnectionString"));
            return client.GetDatabase(config.GetValue<string>("DatabaseSettings:DatabaseName"));
        });

        services.AddSingleton<IExecutionReportCache, ExecutionReportCache>();
        services.AddSingleton<IOrderEngineCache, OrderEngineCache>();
        services.AddSingleton<IOrderReportCache, OrderReportCache>();
        services.AddSingleton<IOrderStopCache, OrderStopCache>();
        services.AddSingleton<IBookOfferCache, BookOfferCache>();

        services.AddSingleton<ISecurityEngineCache, SecurityEngineCache>();

        services.AddSingleton<IOrderEngineRepository, OrderEngineRepository>();
        services.AddSingleton<IOrderEngineContext, OrderEngineContext>();

        services.AddHostedService<OutboxConsumerService<OrderEngine>>();
        services.AddHostedService<OutboxPublisherService<OrderEngine>>();

    }
}