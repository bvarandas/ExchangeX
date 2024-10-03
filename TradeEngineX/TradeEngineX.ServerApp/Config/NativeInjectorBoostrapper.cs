using FluentResults;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Sharedx.Infra.Outbox.Cache;
using Sharedx.Infra.Outbox.Services;
using SharedX.Core.Bus;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Specs;
using System.Reflection;
using TradeEngineX.Application.Commands;
using TradeEngineX.Application.Events;
using TradeEngineX.Core.Interfaces;
using TradeEngineX.Infra.Data;
using TradeEngineX.Infra.Repositories;

namespace TradeEngineX.ServerApp;
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
            x.AddConsumer<OutboxConsumerService<TradeReport>>();
            x.UsingRabbitMq((context, cfg) =>
            {
                string hostname = config["QueueSettings:Hostname"]!;
                string port = config["QueueSettings:port"]!;

                cfg.Host(new Uri("rabbitmq://" + hostname + ":" + port), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ConfigureEndpoints(context);

                cfg.ReceiveEndpoint(config.GetSection("QueueSettings:QueueNameTradeNegine").Value, e =>
                {
                    e.PrefetchCount = 10;
                    e.UseMessageRetry(r => r.Interval(2, 100));
                    e.ConfigureConsumer<OutboxConsumerService<TradeReport>>(context);
                });
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
        services.AddSingleton(typeof(IOutboxPublisherService<>), typeof(OutboxPublisherService<>));
        services.AddSingleton(typeof(IOutboxConsumerService<>), typeof(OutboxConsumerService<>));
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
    }
}