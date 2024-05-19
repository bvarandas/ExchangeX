using System.Reflection;
using MassTransit;
using MediatR;
using Security.API.Consumer;
using SecurityX.Core.Interfaces;
using SecurityX.Infra.Cache;
using SharedX.Core.Bus;
using SharedX.Core.Interfaces;
using SharedX.Core.Specs;


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
            //x.AddConsumer<ConsumerOrdersBusApp>();
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
        //services.AddSingleton<INotificationHandler<DomainNotification>, DomainNotificationHandler>();

        //services.AddSingleton<IRequestHandler<OrderTradeNewEvent, bool>, OrderTradeEventHandler>();
        //services.AddSingleton<IRequestHandler<OrderTradeModifyEvent, bool>, OrderTradeEventHandler>();
        //services.AddSingleton<IRequestHandler<OrderTradeCancelEvent, bool>, OrderTradeEventHandler>();

        // Domain - Command
        //services.AddSingleton<IRequestHandler<OrderTradeCancelCommand, bool>, OrderTradeCommandHandler>();
        //services.AddSingleton<IRequestHandler<OrderTradeCancelReplaceCommand, bool>, OrderTradeCommandHandler>();
        //services.AddSingleton<IRequestHandler<OrderTradeNewCommand, bool>, OrderTradeCommandHandler>();

        // Infra - Data
        services.AddSingleton<ISecurityCache, SecurityCache>();
    
        //services.AddSingleton<IOrderEngineCache, OrderEngineCache>();
        //services.AddSingleton<IOrderReportCache, OrderReportCache>();
        //services.AddSingleton<IOrderStopCache, OrderStopCache>();

        //services.AddHostedService<ConsumerExecutionReportApp>();
        //services.AddHostedService<PublisherOrderReportApp>();
        //services.AddHostedService<PublisherOrderApp>();
        services.AddHostedService<ConsumerSecurityApp>();
    }
}