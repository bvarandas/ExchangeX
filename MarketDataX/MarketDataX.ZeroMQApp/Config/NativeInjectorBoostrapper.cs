using FluentResults;
using MarketDataX.Application.Commands;
using MarketDataX.Core.Interfaces;
using MarketDataX.Infra.Cache;
using MarketDataX.Infra.Data;
using MarketDataX.Infra.Repositories;
using MarketDataX.ServerApp.Receiver;
using MarketDataX.ServerApp.Services;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Sharedx.Infra.Outbox.Cache;
using Sharedx.Infra.Outbox.Services;
using SharedX.Core.Bus;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Specs;
using SharedX.Core.ValueObjects;
using System.Reflection;

namespace DropCopyX.ServerApp;
internal class NativeInjectorBoostrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration config)
    {
        services.AddSwaggerGen();

        services.Configure<QueueSettings>(config.GetSection(nameof(QueueSettings)));
        services.Configure<ConnectionZmq>(config.GetSection(nameof(ConnectionZmq)));
        services.Configure<ConnectionRmq>(config.GetSection(nameof(ConnectionRmq)));

        services.AddMassTransit(x =>
        {
            x.AddConsumer<OutboxConsumerService<EnvelopeOutbox<MarketData>>>();
            x.AddConsumer<OutboxConsumerService<EnvelopeOutbox<Security>>>();

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

        // Services
        services.AddSingleton(typeof(ReceiverMarketData), typeof(IReceiverEngine<MarketData>));
        services.AddSingleton(typeof(ReceiverSecurity), typeof(IReceiverEngine<Security>));

        // Outbox 
        services.AddSingleton(typeof(IOutboxConsumerService<EnvelopeOutbox<MarketData>>),
                               typeof(OutboxConsumerService<EnvelopeOutbox<MarketData>>));

        services.AddSingleton(typeof(IOutboxConsumerService<EnvelopeOutbox<Security>>),
                               typeof(OutboxConsumerService<EnvelopeOutbox<Security>>));

        //services.AddSingleton(typeof(IOutboxBackgroundService<>), typeof(OutboxBackgroundService<>));
        services.AddSingleton(typeof(IOutboxCache<>), typeof(OutboxCache<>));

        // Domain - Events
        //services.AddSingleton<INotificationHandler<ExecutedTradeEvent>, ExecutedTradeEventHandler>();

        // Domain - Commands
        services.AddSingleton<IRequestHandler<SnapshotCommand, Result>, SnapshotCommandHandler>();

        // Infra - Data
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = new MongoClient(config.GetValue<string>("DatabaseSettings:ConnectionString"));
            return client.GetDatabase(config.GetValue<string>("DatabaseSettings:DatabaseName"));
        });

        services.AddSingleton<IFixSessionMarketDataCache, FixSessionMarketDataCache>();
        services.AddSingleton<IMarketDataContext, MarketDataContext>();
        services.AddSingleton<IMarketDataRepository, MarketDataRepository>();

        services.AddHostedService<PublisherFixApp>();
    }
}