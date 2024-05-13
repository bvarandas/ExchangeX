using System.Reflection;
using MarketDataX.Application.Commands;
using MassTransit;
using MediatR;
using OrderEngineX.API.Consumers;
using OrderEngineX.API.Publishers;
using OrderEngineX.Application.Commands;
using OrderEngineX.Application.Events;
using OrderEngineX.Core.Interfaces;
using OrderEngineX.Core.Notifications;
using OrderEngineX.Infra.Cache;
using OrderX.API.Consumers;
using SharedX.Core.Bus;
using SharedX.Core.Interfaces;
using SharedX.Core.Specs;
using Sharex.Infra.Order.Cache;

namespace DropCopyX.ServerApp;
internal class NativeInjectorBoostrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration config)
    {
        services.AddSwaggerGen();

        services.Configure<QueueCommandSettings>(config.GetSection(nameof(QueueCommandSettings)));
        services.Configure<ConnectionZmq>(config.GetSection(nameof(ConnectionZmq)));

        services.AddMassTransit(x =>
        {
            x.AddConsumer<ConsumerOrdersBusApp>();
            x.UsingRabbitMq((context, cfg) =>
            {
                string hostname = config["QueueCommandSettings:Hostname"]!;
                string port = config["QueueCommandSettings:port"]!;

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

        // Domain - Events
        services.AddSingleton<INotificationHandler<DomainNotification>, DomainNotificationHandler>();

        services.AddSingleton<IRequestHandler<OrderTradeNewEvent, bool>, OrderTradeEventHandler>();
        services.AddSingleton<IRequestHandler<OrderTradeModifyEvent, bool>, OrderTradeEventHandler>();
        services.AddSingleton<IRequestHandler<OrderTradeCancelEvent, bool>, OrderTradeEventHandler>();

        // Domain - Command
        services.AddSingleton<IRequestHandler<OrderTradeCancelCommand, bool>, OrderTradeCommandHandler>();
        services.AddSingleton<IRequestHandler<OrderTradeCancelReplaceCommand, bool>, OrderTradeCommandHandler>();
        services.AddSingleton<IRequestHandler<OrderTradeNewCommand, bool>, OrderTradeCommandHandler>();

        // Infra - Data
        services.AddSingleton<IExecutionReportCache, ExecutionReportCache>();
        services.AddSingleton<IOrderEngineCache, OrderEngineCache>();
        services.AddSingleton<IOrderReportCache, OrderReportCache>();
        services.AddSingleton<IOrderStopCache, OrderStopCache>();

        services.AddHostedService<ConsumerExecutionReportApp>();
        services.AddHostedService<PublisherOrderReportApp>();
        services.AddHostedService<PublisherOrderApp>();
        services.AddHostedService<ConsumerOrdersApp>();
    }
}