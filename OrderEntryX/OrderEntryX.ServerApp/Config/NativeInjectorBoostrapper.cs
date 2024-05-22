using System.Reflection;
using DropCopyX.Infra.Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderEntryX.Core.Interfaces;
using OrderEntryX.Infra.Client;
using OrderEntryX.Infra.Data;
using OrderEntryX.ServerApp.Services;
using Sharedx.Infra.LoginFix.Data;
using SharedX.Core.Bus;
using SharedX.Core.Repositories;
using SharedX.Core.Specs;
using SharedX.Infra.Repositories;
namespace OrderEntryX.ServerApp;
internal class NativeInjectorBoostrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration config)
    {
        services.AddSwaggerGen();

        services.Configure<ConnectionZmq>(config.GetSection(nameof(ConnectionZmq)));
        services.Configure<ConnectionRedis>(config.GetSection(nameof(ConnectionRedis)));
        
        // FIX - Application
        services.AddSingleton<IFixServerApp, FixServerApp>();
        

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
        
        services.AddSingleton<ILoginRepository, LoginFixRepository>();
        services.AddSingleton<ILoginFixContext, LoginFixContext>();

        services.AddSingleton<IOrderEntryChache, OrderEntryChache>();
        services.AddSingleton<IOrderEntryContext, OrderEntryContext>();

        services.AddHostedService<PublisherOrdersApp>();
        services.AddHostedService<ConsumerFixApp>();
    }
}