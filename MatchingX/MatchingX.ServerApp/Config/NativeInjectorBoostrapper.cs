using System.Reflection;
using MacthingX.Application.Commands;
using MacthingX.Application.Commands.Match.OrderStatus;
using MacthingX.Application.Commands.Match.OrderType;
using MacthingX.Application.Events;
using MacthingX.Application.Interfaces;
using MacthingX.Application.Querys;
using MacthingX.Application.Services;
using MatchingX.Core.Interfaces;
using MatchingX.Core.Notifications;
using MatchingX.Core.Repositories;
using MatchingX.Infra.Cache;
using MatchingX.Infra.Data;
using MatchingX.Infra.FixClientApp;
using MatchingX.Infra.Repositories;
using MatchingX.ServerApp.Consumer;
using MatchingX.ServerApp.Publisher;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sharedx.Infra.Order.Cache;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Specs;
using SharedX.Infra.Cache;
using SharedX.Infra.Order.Data;
using SharedX.Infra.Order.Repositories;

namespace MatchinX.API.Config;
internal class NativeInjectorBoostrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration config)
    {
        services.AddSwaggerGen();

        services.Configure<QueueCommandSettings>(config.GetSection(nameof(QueueCommandSettings)));
        services.Configure<ConnectionRedis>(config.GetSection(nameof(ConnectionRedis)));
        services.Configure<ConnectionZmq>(config.GetSection(nameof(ConnectionZmq)));
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

        // Domain - Events
        services.AddSingleton<INotificationHandler<DomainNotification>, DomainNotificationHandler>();

        services.AddSingleton<INotificationHandler<ExecutedTradeEvent>, ExecutedTradeEventHandler>();

        services.AddSingleton<INotificationHandler<OrderCanceledEvent>, OrderEventHandler>();
        services.AddSingleton<INotificationHandler<OrderTradedEvent>, OrderEventHandler>();
        services.AddSingleton<INotificationHandler<OrderOpenedEvent>, OrderEventHandler>();
        
        // Domain - Querys
        services.AddSingleton<IRequestHandler<GetTradeIdQuery, Trade>, GetTradeQueryHandler>();
        services.AddSingleton<IRequestHandler<GetOrderQuery, IEnumerable<OrderEngine>>, GetOrderQueryHandler>();

        // Domain - Commands
        services.AddSingleton<IRequestHandler<MatchingLimitCommand, (OrderStatus, Dictionary<long, OrderEngine>)>, MatchingCommandHandler>();
        services.AddSingleton<IRequestHandler<MatchingMarketCommand, (OrderStatus, Dictionary<long, OrderEngine>)>, MatchingCommandHandler>();
        services.AddSingleton<IRequestHandler<MatchingStopLimitCommand, (OrderStatus, Dictionary<long, OrderEngine>)>, MatchingCommandHandler>();
        services.AddSingleton<IRequestHandler<MatchingStopCommand, (OrderStatus, Dictionary<long, OrderEngine>)>, MatchingCommandHandler>();

        services.AddSingleton<IRequestHandler<MatchingOpenedCommand,bool>, MatchingStatusCommandHandler>();
        services.AddSingleton<IRequestHandler<MatchingFilledCommand, bool>, MatchingStatusCommandHandler>();
        services.AddSingleton<IRequestHandler<MatchingPartiallyFilledCommand, bool>, MatchingStatusCommandHandler>();
        services.AddSingleton<IRequestHandler<MatchingCancelCommand, bool>, MatchingStatusCommandHandler>();

        // Domain - Services
        services.AddSingleton<IMatchingReceiver,    MatchingReceiver>();
        services.AddSingleton<ITradeOrderService,   TradeOrderService>();

        var strategies = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IMatch).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);
        
        foreach (var strategy in strategies)
            services.AddSingleton(typeof(IMatch), strategy);
        
        /*
        services.AddSingleton<IMatch, MatchLimit>();
        services.AddSingleton<IMatch, MatchStop>();
        services.AddSingleton<IMatch, MatchStopLimit>();
        services.AddSingleton<IMatch, MatchMarket>();
        */
        // Infra - Data
        services.AddSingleton<IDropCopyCache, DropCopyCache>();
        services.AddSingleton<IMarketDataCache, MarketDataCache>();

        services.AddSingleton<IOrderStopCache, OrderStopCache>();

        services.AddSingleton<IBookOfferCache, BookOfferCache>();
        services.AddSingleton<IMatchingCache, MatchingCache>();
        services.AddSingleton<IMatchContextStrategy, MatchContextStrategy>();
        

        services.AddSingleton<IOrderRepository, OrderRepository>();
        services.AddSingleton<IExecutedTradeRepository, ExecutedTradeRepository>();

        services.AddSingleton<ITradeRepository, TradeRepository>();
        services.AddSingleton<ITradeContext, TradeContext>();

        services.AddSingleton<IOrderContext, OrderContext>();
        services.AddSingleton<IExecutedTradeContext ,ExecutedTradeContext >();
        services.AddSingleton<IMatchingRepository, MatchingRepository>();
        services.AddSingleton<IMatchingContext, MatchingContext>();
        
        // Apps - Services
        services.AddHostedService<ConsumerOrderApp>();
        services.AddHostedService<PublisherMarketDataApp>();
        services.AddHostedService<PublisherOrderEngineApp>();
        services.AddHostedService<PublisherDropCopyApp>();
    }
}