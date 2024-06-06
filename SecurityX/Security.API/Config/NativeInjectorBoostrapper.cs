using System.Reflection;
using MediatR;
using Security.API.Consumer;
using Security.API.Publisher;
using Security.Application.Commands;
using Security.Application.Events;
using Security.Application.Services;
using Security.Infra.Repositories;
using SecurityX.Core.Interfaces;
using SecurityX.Core.Notifications;
using SecurityX.Infra.Cache;
using SharedX.Core.Bus;
using SharedX.Core.Specs;
namespace DropCopyX.ServerApp;
internal class NativeInjectorBoostrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration config)
    {
        services.AddSwaggerGen();

        services.Configure<QueueSettings>(config.GetSection(nameof(QueueSettings)));
        services.Configure<ConnectionZmq>(config.GetSection(nameof(ConnectionZmq)));
                
        // FIX - Application

        //services.AddSingleton<IFixServerApp, FixServerApp>();
        
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

        // Domain - Commands
        services.AddSingleton<IRequestHandler<SecurityNewCommand, bool>, SecurityEngineCommandHandler>();
        services.AddSingleton<IRequestHandler<SecurityRemoveCommand, bool>, SecurityEngineCommandHandler>();
        services.AddSingleton<IRequestHandler<SecurityUpdateCommand, bool>, SecurityEngineCommandHandler>();

        //Domain - Events 
        services.AddSingleton<INotificationHandler<SecurityChangedEvent>, SecurityEngineEventHandler>();

        // Service
        services.AddSingleton<ISecurityService, SecurityService>();

        // Infra - Data
        services.AddSingleton<ISecurityCache, SecurityCache>();
        services.AddSingleton<ISecurityRepository, SecurityRepository>();

        services.AddHostedService<PublisherSecurityApp>();
        services.AddHostedService<ConsumerSecurityApp>();
    }
}