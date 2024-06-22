using System.Reflection;
using MassTransit;
using MediatR;
using OrderEngineX.API.Consumers;
using OrderEngineX.API.Publishers;
using OrderEngineX.Application.Commands;
using OrderEngineX.Application.Events;
using OrderEngineX.Core.Interfaces;
using OrderEngineX.Core.Notifications;
using OrderEngineX.Infra.Cache;
using SharedX.Core.Bus;
using SharedX.Core.Interfaces;
using SharedX.Core.Specs;
using Sharedx.Infra.Order.Cache;
using OrderEngineX.Application.Commands.Order;
using SharedX.Infra.Cache;
using SharedX.Infra.Order.Repositories;
using SharedX.Infra.Order.Data;
using Sharedx.Infra.Outbox.Services;
using Sharedx.Infra.Outbox.Cache;
using FluentResults;
using MongoDB.Driver;

namespace OrderEngineX.API.Config;
internal class NativeInjectorBoostrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration config)
    {
        services.AddSwaggerGen();

        services.Configure<QueueSettings>(config.GetSection(nameof(QueueSettings)));
        services.Configure<ConnectionZmq>(config.GetSection(nameof(ConnectionZmq)));
        services.Configure<ConnectionRedis>(config.GetSection(nameof(ConnectionRedis)));

        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderEngineOutboxApp>();
            x.AddConsumer<DropCopyOutboxApp>();

            x.UsingRabbitMq((context, cfg) =>
            {
                
                string hostname = config["QueueSettings:Hostname"]!;
                string port = config["QueueSettings:port"]!;

                cfg.Host(hostname,port , "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
                cfg.ConfigureEndpoints(context);
            });
        });
        
        // FIX - Application

        //services.AddSingleton<IFixServerApp, FixServerApp>();
        
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
        services.AddSingleton(typeof(IOutboxBackgroundService<>), typeof(OutboxBackgroundService<>));
        services.AddSingleton(typeof(IOutboxCache<>), typeof(OutboxCache<>));

        // Domain - Events
        services.AddSingleton<INotificationHandler<DomainNotification>, DomainNotificationHandler>();

        services.AddSingleton<INotificationHandler<OrderTradeNewEvent>, OrderTradeEventHandler>();
        services.AddSingleton<INotificationHandler<OrderTradeModifyEvent>, OrderTradeEventHandler>();
        services.AddSingleton<INotificationHandler<OrderTradeCancelEvent>, OrderTradeEventHandler>();

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

        services.AddSingleton<IOrderRepository, OrderRepository>();
        services.AddSingleton<IOrderContext, OrderContext>();

        services.AddHostedService<ConsumerExecutionReportApp>();
        //services.AddHostedService<PublisherOrderReportApp>();
        services.AddHostedService<PublisherOrderApp>();
        services.AddHostedService<ConsumerOrdersApp>();
    }
}