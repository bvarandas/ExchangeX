using System.Reflection;
using MarketDataX.Core.Interfaces;
using MarketDataX.ServerApp.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MarketDataX.Core.Repositories;
using MarketDataX.Infra.Data;
using SharedX.Core.Bus;
using SharedX.Core.Specs;
using MarketDataX.ServerApp.Consumer;
using MarketDataX.Infra.Cache;

namespace DropCopyX.ServerApp;
internal class NativeInjectorBoostrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration config)
    {
        services.AddSwaggerGen();

        services.Configure<QueueCommandSettings>(config.GetSection(nameof(QueueCommandSettings)));
        services.Configure<ConnectionZmq>(config.GetSection(nameof(ConnectionZmq)));

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
        services.AddSingleton<IFixSessionMarketDataCache, FixSessionMarketDataCache>();
        //services.AddSingleton<IOrderEntryRepository, OrderEntryRepository>();

        services.AddHostedService<ConsumerMarketDataApp>();
        services.AddHostedService<PublisherFixApp>();
    }
}