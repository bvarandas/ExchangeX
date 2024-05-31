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
        services.AddSingleton(typeof(IManagerOutboxApp<>), typeof( ManagerOutboxApp<>));

        // Domain - Events
        services.AddSingleton<INotificationHandler<DomainNotification>, DomainNotificationHandler>();

        services.AddSingleton<INotificationHandler<OrderTradeNewEvent>, OrderTradeEventHandler>();
        services.AddSingleton<INotificationHandler<OrderTradeModifyEvent>, OrderTradeEventHandler>();
        services.AddSingleton<INotificationHandler<OrderTradeCancelEvent>, OrderTradeEventHandler>();

        // Domain - Command
        services.AddSingleton<IRequestHandler<OrderCancelCommand, bool>, OrderEngineCommandHandler>();
        services.AddSingleton<IRequestHandler<OrderCancelReplaceCommand, bool>, OrderEngineCommandHandler>();
        services.AddSingleton<IRequestHandler<OrderOpenedCommand, bool>, OrderEngineCommandHandler>();

        // Infra - Data
        services.AddSingleton<IExecutionReportCache, ExecutionReportCache>();
        services.AddSingleton<IOrderEngineCache, OrderEngineCache>();
        services.AddSingleton<IOrderReportCache, OrderReportCache>();
        services.AddSingleton<IOrderStopCache, OrderStopCache>();
        services.AddSingleton<IMatchingCache, MatchingCache>();

        services.AddSingleton<IOrderRepository, OrderRepository>();
        services.AddSingleton<IOrderContext, OrderContext>();

        services.AddHostedService<ConsumerExecutionReportApp>();
        //services.AddHostedService<PublisherOrderReportApp>();
        services.AddHostedService<PublisherOrderApp>();
        services.AddHostedService<ConsumerOrdersApp>();
    }
}