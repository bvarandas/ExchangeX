using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderEntryX.Core.Repositories;
using OrderEntryX.Infra.Client;
using OrderEntryX.Infra.Data;
using OrderEntryX.Infra.Repositories;
using OrderEntryX.ServerApp.Services;
using SharedX.Core.Bus;
using SharedX.Core.Specs;
namespace OrderEntryX.ServerApp;
internal class NativeInjectorBoostrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration config)
    {
        services.AddSwaggerGen();

        services.Configure<QueueCommandSettings>(config.GetSection(nameof(QueueCommandSettings)));
        services.Configure<ConnectionZmq>(config.GetSection(nameof(ConnectionZmq)));

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
        
        
        services.AddSingleton<IOrderEntryContext, OrderEntryContext>();
        services.AddSingleton<IOrderEntryRepository, OrderEntryRepository>();

        services.AddHostedService<PublisherOrdersApp>();
        services.AddHostedService<PublisherFixApp>();
    }
}