using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedX.Core.Bus;
using SharedX.Core.Specs;
using MassTransit;
using Sharedx.Infra.Outbox.Services;
using SharedX.Core.Interfaces;
using Sharedx.Infra.Outbox.Cache;
using MediatR;
using FluentResults;
using MongoDB.Driver;
using TradeEngineX.Application.Commands;
using TradeEngineX.Application.Events;
using TradeEngineX.Infra.Data;
using TradeEngineX.Core.Interfaces;
using TradeEngineX.Infra.Repositories;
using TradeEngineX.ServerApp.Consumer;

namespace TradeEngineX.ServerApp;
internal class NativeInjectorBoostrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration config)
    {
        services.AddSwaggerGen();

        services.Configure<QueueSettings>(config.GetSection(nameof(QueueSettings)));
        services.Configure<ConnectionZmq>(config.GetSection(nameof(ConnectionZmq)));

        services.AddMassTransit(x =>
        {
            x.AddConsumer(typeof(IOutboxConsumerService<>), typeof(OutboxConsumerService<>));
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
        services.AddSingleton<INotificationHandler<TradeEngineCreatedEvent>, TradeEngineEventHandler>();
        services.AddSingleton<INotificationHandler<TradeEngineUpdatedEvent>, TradeEngineEventHandler>();
        services.AddSingleton<INotificationHandler<TradeEngineRemovedEvent>, TradeEngineEventHandler>();

        // Domain - Commands
        services.AddSingleton<IRequestHandler<TradeEngineNewCommand, Result>, TradeEngineCommandHandler>();
        services.AddSingleton<IRequestHandler<TradeEngineRemoveCommand, Result>, TradeEngineCommandHandler>();
        services.AddSingleton<IRequestHandler<TradeEngineUpdateCommand, Result>, TradeEngineCommandHandler>();

        // Infra - Data
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = new MongoClient(config.GetValue<string>("DatabaseSettings:ConnectionString"));
            return client.GetDatabase(config.GetValue<string>("DatabaseSettings:DatabaseName"));
        });

        services.AddSingleton<ITradeEngineContext, TradeEngineContext>();
        services.AddSingleton<ITradeEngineRepository, TradeEngineRepository>();

        services.AddHostedService<ConsumerTradeEngineApp>();
    }
}