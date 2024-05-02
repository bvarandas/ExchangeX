using System.Reflection;
using MassTransit;
using OrderX.API.Consumer;
using SharedX.Core.Bus;
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
            x.AddConsumer<ConsumerOrders>();
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
        //services.AddSingleton<INotificationHandler<ExecutedTradeEvent>, ExecutedTradeEventHandler>();

        //services.AddSingleton<IRequestHandler<ExecutionReportCommand, bool>, ExecutionReportCommandHandler>();
        //services.AddSingleton<INotificationHandler<OrderFilledEvent>, OrderEventHandler>();
        //services.AddSingleton<INotificationHandler<OrderOpenedEvent>, OrderEventHandler>();
        //services.AddSingleton<INotificationHandler<OrderRejectedEvent>, OrderEventHandler>();

        // Infra - Data
        //services.AddSingleton<IExecutedTradeCache, ExecutedTradeCache>();
        //services.AddSingleton<IExecutionReportChache, ExecutionReportChache>();
        //services.AddSingleton<IDropCopyContext, DropCopyContext>();
        //services.AddSingleton<IDropCopyRepository , DropCopyRepository>();

        services.AddHostedService<ConsumerOrdersApp>();
        //services.AddHostedService<PublisherFixApp>();
    }
}