using System.Reflection;
using FluentResults;
using MacthingX.Application.Commands;
using MacthingX.Application.Events;
using MatchingX.Core.Repositories;
using MatchingX.Infra.Data;
using MatchingX.Infra.Repositories;
using MediatR;
using SharedX.Core.Bus;
using SharedX.Core.Matching;
using SharedX.Core.Specs;
using ShareX.Core.Interfaces;

namespace MatchinX.API.Configurations;

internal class NativeInjectorBoostrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration config)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.Configure<QueueCommandSettings>(config.GetSection(nameof(QueueCommandSettings)));

        // Domain Bus (Mediator)
        services.AddSingleton<IMediatorHandler, InMemmoryBus>();
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

        // Domain - Commands
        services.AddSingleton<IRequestHandler<OrderFilledCommand, bool>, OrderCommandHandler>();
        services.AddSingleton<IRequestHandler<OrderRejectedCommand, bool>, OrderCommandHandler>();
        services.AddSingleton<IRequestHandler<OrderCanceledCommand, bool>, OrderCommandHandler>();
        services.AddSingleton<IRequestHandler<OrderCanceledCommand, bool>, OrderCommandHandler>();

        // Infra - Data

        services.AddSingleton<IOrderRepository, OrderRepository>();
        services.AddSingleton<IExecutedTradeRepository, ExecutedTradeRepository>();

        services.AddSingleton<IOrderContext, OrderContext>();
        services.AddSingleton<IExecutedTradeContext ,ExecutedTradeContext >();
        //services.AddHostedService<WorkerConsumeBitstamp>();
    }
}
