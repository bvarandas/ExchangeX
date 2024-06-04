using System.Reflection;
using MarketDataX.Core.Interfaces;
using MarketDataX.ServerApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedX.Core.Bus;
using SharedX.Core.Specs;
using MarketDataX.ServerApp.Consumer;
using MarketDataX.Infra.Cache;
using MassTransit;
using Sharedx.Infra.Outbox.Services;
using SharedX.Core.Interfaces;
using Sharedx.Infra.Outbox.Cache;
using MarketDataX.Application.Commands;
using MediatR;
using MarketDataX.Infra.Data;
using MarketDataX.Infra.Repositories;

namespace DropCopyX.ServerApp;
internal class NativeInjectorBoostrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration config)
    {
        services.AddSwaggerGen();

        services.Configure<QueueSettings>(config.GetSection(nameof(QueueSettings)));
        services.Configure<ConnectionZmq>(config.GetSection(nameof(ConnectionZmq)));

        services.AddMassTransit(x =>
        {
            x.AddConsumer<SecurityEngineOutboxApp>();
            x.AddConsumer<MarketDataOutboxApp>();

            x.UsingRabbitMq((context, cfg) =>
            {
                string hostname = config["QueueSettings:Hostname"]!;
                string port = config["QueueSettings:port"]!;

                cfg.Host(hostname, port, "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });

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
        // Outbox 
        services.AddSingleton(typeof(IOutboxBackgroundService<>), typeof(OutboxBackgroundService<>));
        services.AddSingleton(typeof(IOutboxCache<>), typeof(OutboxCache<>));

        // Domain - Events
        //services.AddSingleton<INotificationHandler<ExecutedTradeEvent>, ExecutedTradeEventHandler>();

        // Domain - Commands
        services.AddSingleton<IRequestHandler<SnapshotCommand, bool>, SnapshotCommandHandler>();

        // Infra - Data
        services.AddSingleton<IFixSessionMarketDataCache, FixSessionMarketDataCache>();
        services.AddSingleton<IMarketDataContext, MarketDataContext>();
        services.AddSingleton<IMarketDataRepository, MarketDataRepository>();

        services.AddHostedService<ConsumerMarketDataApp>();
        services.AddHostedService<PublisherFixApp>();
    }
}