﻿using FluentResults;
using MacthingX.Application.Commands.Match.OrderStatus;
using MacthingX.Application.Commands.Match.OrderType;
using MacthingX.Application.Events;
using MacthingX.Application.Handlers;
using MacthingX.Application.Interfaces;
using MacthingX.Application.Services;
using MassTransit;
using MatchingX.Core.Interfaces;
using MatchingX.Core.Notifications;
using MatchingX.Core.Repositories;
using MatchingX.Infra.Cache;
using MatchingX.Infra.Data;
using MatchingX.Infra.FixClientApp;
using MatchingX.Infra.Repositories;
using MatchingX.ServerApp.Publisher;
using MatchingX.ServerApp.Receiver;
using Medallion.Threading;
using Medallion.Threading.ZooKeeper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Sharedx.Infra.Order.Cache;
using Sharedx.Infra.Outbox.Cache;
using Sharedx.Infra.Outbox.Services;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Specs;
using SharedX.Core.ValueObjects;
using SharedX.Infra.Cache;
using System.Reflection;

namespace MatchinX.API.Config;
internal class NativeInjectorBoostrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration config)
    {
        services.AddSwaggerGen();

        services.Configure<QueueSettings>(config.GetSection(nameof(QueueSettings)));
        services.Configure<ConnectionRedis>(config.GetSection(nameof(ConnectionRedis)));
        services.Configure<ConnectionZmq>(config.GetSection(nameof(ConnectionZmq)));
        services.Configure<ConnectionRmq>(config.GetSection(nameof(ConnectionRmq)));
        services.Configure<ConnectionZooKeeper>(config.GetSection(nameof(ConnectionZooKeeper)));

        services.AddMassTransit(x =>
        {
            x.AddConsumer<OutboxConsumerService<MarketData>>();
            x.AddConsumer<OutboxConsumerService<Security>>();

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

                cfg.Message<EnvelopeOutbox<OrderEngine>>(x =>
                {
                    x.SetEntityName("outbox-order-engine");
                });

                cfg.Message<EnvelopeOutbox<Security>>(x =>
                {
                    x.SetEntityName("outbox-security-engine");
                });

                cfg.ReceiveEndpoint(config.GetSection("QueueSettings:QueueNameMatching").Value, e =>
                {
                    e.PrefetchCount = 10;
                    e.UseMessageRetry(r => r.Interval(2, 100));
                    e.ConfigureConsumer<OutboxConsumerService<MarketData>>(context);
                });

                cfg.ReceiveEndpoint(config.GetSection("QueueSettings:QueueNameSecurity").Value, e =>
                {
                    e.PrefetchCount = 10;
                    e.UseMessageRetry(r => r.Interval(2, 100));
                    e.ConfigureConsumer<OutboxConsumerService<Security>>(context);
                });
            });
        });

        // FIX - Application
        services.AddSingleton<ITradeClientApp, TradeClientApp>();
        //services.AddSingleton<IApplication, FixServerApp>();

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

        // ZooKeeper - DistributedSynchronizator
        services.AddSingleton<IDistributedLockProvider>(
            new ZooKeeperDistributedSynchronizationProvider(config["ConnectionZooKeeper:ConnectionString"]!,
                options => options.ConnectTimeout(TimeSpan.FromSeconds(5))));

        // Services
        services.AddSingleton(typeof(IReceiverEngine<OrderEngine>), typeof(ReceiverOrder));

        services.AddSingleton(typeof(IPublisherEngine<ExecutionReport>), typeof(PublisherDropCopy));
        services.AddSingleton(typeof(IPublisherEngine<MarketData>), typeof(PublisherMarketData));
        services.AddSingleton(typeof(IPublisherEngine<ExecutionReport>), typeof(PublisherOrderEngine));

        // Outbox 
        services.AddSingleton(typeof(IOutboxConsumerService<EnvelopeOutbox<MarketData>>),
                               typeof(OutboxConsumerService<EnvelopeOutbox<MarketData>>));

        services.AddSingleton(typeof(IOutboxConsumerService<EnvelopeOutbox<Security>>),
                               typeof(OutboxConsumerService<EnvelopeOutbox<Security>>));

        services.AddSingleton(typeof(IOutboxCache<OrderEngine>), typeof(OutboxCache<OrderEngine>));
        services.AddSingleton(typeof(IOutboxCache<TradeReport>), typeof(OutboxCache<TradeReport>));
        services.AddSingleton(typeof(IOutboxCache<ExecutionReport>), typeof(OutboxCache<ExecutionReport>));
        services.AddSingleton(typeof(IOutboxCache<MarketData>), typeof(OutboxCache<MarketData>));

        // Domain - Events
        services.AddSingleton<INotificationHandler<DomainNotification>, DomainNotificationHandler>();
        services.AddSingleton<INotificationHandler<ExecutedTradeEvent>, ExecutedTradeEventHandler>();

        services.AddSingleton<INotificationHandler<OrderCanceledEvent>, OrderEventHandler>();
        services.AddSingleton<INotificationHandler<OrderTradedEvent>, OrderEventHandler>();
        services.AddSingleton<INotificationHandler<OrderOpenedEvent>, OrderEventHandler>();

        // Domain - Commands
        services.AddSingleton<IRequestHandler<MatchingLimitCommand, (OrderStatus, Dictionary<long, OrderEngine>)>, MatchingCommandHandler>();
        services.AddSingleton<IRequestHandler<MatchingMarketCommand, (OrderStatus, Dictionary<long, OrderEngine>)>, MatchingCommandHandler>();
        services.AddSingleton<IRequestHandler<MatchingStopLimitCommand, (OrderStatus, Dictionary<long, OrderEngine>)>, MatchingCommandHandler>();
        services.AddSingleton<IRequestHandler<MatchingStopCommand, (OrderStatus, Dictionary<long, OrderEngine>)>, MatchingCommandHandler>();

        services.AddSingleton<IRequestHandler<MatchingOpenedCommand, Result>, MatchingStatusCommandHandler>();
        services.AddSingleton<IRequestHandler<MatchingFilledCommand, Result>, MatchingStatusCommandHandler>();
        services.AddSingleton<IRequestHandler<MatchingPartiallyFilledCommand, Result>, MatchingStatusCommandHandler>();
        services.AddSingleton<IRequestHandler<MatchingCancelCommand, Result>, MatchingStatusCommandHandler>();

        // Domain - Services
        services.AddSingleton<IMatchingReceiver, MatchingReceiver>();
        services.AddSingleton<ITradeOrderService, TradeOrderService>();

        var strategies = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IMatch).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

        foreach (var strategy in strategies)
            services.AddSingleton(typeof(IMatch), strategy);


        // Infra - Data
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = new MongoClient(config.GetValue<string>("DatabaseSettings:ConnectionString"));
            return client.GetDatabase(config.GetValue<string>("DatabaseSettings:DatabaseName"));
        });

        services.AddSingleton<IOrderStopCache, OrderStopCache>();

        services.AddSingleton<IBookOfferCache, BookOfferCache>();
        services.AddSingleton<IMatchingCache, MatchingCache>();
        services.AddSingleton<IMatchContextStrategy, MatchContextStrategy>();

        services.AddSingleton<IExecutedTradeRepository, ExecutedTradeRepository>();

        services.AddSingleton<ITradeRepository, TradeRepository>();
        services.AddSingleton<ITradeContext, TradeContext>();

        services.AddSingleton<IExecutedTradeContext, ExecutedTradeContext>();
        services.AddSingleton<IMatchingRepository, MatchingRepository>();
        services.AddSingleton<IMatchingContext, MatchingContext>();
        //services.AddSingleton<ISecurityCache, SecurityCache>();

        services.AddHostedService<OutboxConsumerService<OrderEngine>>();
        //services.AddHostedService<OutboxPublisherService<OrderEngine>>();


    }
}