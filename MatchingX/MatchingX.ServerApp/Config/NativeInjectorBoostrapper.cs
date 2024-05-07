using System.Reflection;
using MacthingX.Application.Commands;
using MacthingX.Application.Events;
using MacthingX.Application.Interfaces;
using MacthingX.Application.Querys;
using MacthingX.Application.Services;
using MatchingX.Core.Interfaces;
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
using SharedX.Core.Bus;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching;
using SharedX.Core.Specs;
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
        services.AddSingleton<INotificationHandler<ExecutedTradeEvent>, ExecutedTradeEventHandler>();

        services.AddSingleton<INotificationHandler<OrderCanceledEvent>, OrderEventHandler>();
        services.AddSingleton<INotificationHandler<OrderFilledEvent>, OrderEventHandler>();
        services.AddSingleton<INotificationHandler<OrderOpenedEvent>, OrderEventHandler>();
        services.AddSingleton<INotificationHandler<OrderRejectedEvent>, OrderEventHandler>();

        // Domain - Querys
        services.AddSingleton<IRequestHandler<GetTradeIdQuery, Trade>, GetTradeQueryHandler>();
        services.AddSingleton<IRequestHandler<GetOrderQuery, IEnumerable<OrderEng>>, GetOrderQueryHandler>();

        // Domain - Commands
        services.AddSingleton<IRequestHandler<OrderFilledCommand, bool>, OrderCommandHandler>();
        services.AddSingleton<IRequestHandler<OrderRejectedCommand, bool>, OrderCommandHandler>();
        services.AddSingleton<IRequestHandler<OrderCanceledCommand, bool>, OrderCommandHandler>();
        services.AddSingleton<IRequestHandler<OrderCanceledCommand, bool>, OrderCommandHandler>();

        // Domain - Services
        services.AddSingleton<IMatchingReceiver, MatchingReceiver>();
        services.AddSingleton<IMatchLimit,       MatchLimit>();
        services.AddSingleton<IMatchStop,        MatchStop>();
        services.AddSingleton<IMatchStopLimit,   MatchStopLimit>();
        services.AddSingleton<IMatchMarket,      MatchMarket>();
        

        // Infra - Data
        services.AddSingleton<IDropCopyCache, DropCopyCache>();
        services.AddSingleton<IMarketDataCache, MarketDataCache>();

        services.AddSingleton<IOrderRepository, OrderRepository>();
        services.AddSingleton<IExecutedTradeRepository, ExecutedTradeRepository>();

        services.AddSingleton<ITradeRepository, TradeRepository>();
        services.AddSingleton<ITradeContext, TradeContext>();

        services.AddSingleton<IOrderContext, OrderContext>();
        services.AddSingleton<IExecutedTradeContext ,ExecutedTradeContext >();

        services.AddHostedService<ConsumerOrderApp>();
        services.AddHostedService<PublisherMarketDataApp>();
        services.AddHostedService<PublisherOrderEngineApp>();
        services.AddHostedService<PublisherDropCopyApp>();
    }
}