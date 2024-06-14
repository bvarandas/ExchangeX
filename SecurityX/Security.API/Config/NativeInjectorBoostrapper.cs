using System.Reflection;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Security.API.Consumer;
using Security.API.Publisher;
using Security.Application.Commands;
using Security.Application.Events;
using Security.Application.Services;
using Security.Infra.Data;
using Security.Infra.Repositories;
using SecurityX.Core.Interfaces;
using SecurityX.Core.Notifications;
using SecurityX.Infra.Cache;
using SharedX.Core.Bus;
using SharedX.Core.Specs;
namespace SecurityX.ServerApp;
internal class NativeInjectorBoostrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration config)
    {
        services.AddSwaggerGen();

        services.Configure<QueueSettings>(config.GetSection(nameof(QueueSettings)));
        services.Configure<ConnectionZmq>(config.GetSection(nameof(ConnectionZmq)));
        services.Configure<ConnectionRedis>(config.GetSection(nameof(ConnectionRedis)));

        // FIX - Application

        //services.AddSingleton<IFixServerApp, FixServerApp>();

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
        
        // Domain - Commands
        services.AddSingleton<IRequestHandler<SecurityNewCommand, Result>, SecurityEngineCommandHandler>();
        services.AddSingleton<IRequestHandler<SecurityRemoveCommand, Result>, SecurityEngineCommandHandler>();
        services.AddSingleton<IRequestHandler<SecurityUpdateCommand, Result>, SecurityEngineCommandHandler>();

        //Domain - Events 
        services.AddSingleton<INotificationHandler<DomainNotification>, DomainNotificationHandler>();
        services.AddSingleton<INotificationHandler<SecurityChangedEvent>, SecurityEngineEventHandler>();

        // Service
        services.AddSingleton<ISecurityService, SecurityService>();

        // Infra - Data
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = new MongoClient(config.GetValue<string>("DatabaseSettings:ConnectionString"));
            return client.GetDatabase(config.GetValue<string>("DatabaseSettings:DatabaseName"));
        });

        services.AddSingleton<ISecurityEngineContext, SecurityEngineContext>();
        services.AddSingleton<ISecurityCache, SecurityCache>();
        services.AddSingleton<ISecurityEngineRepository, SecurityEngineRepository>();

        services.AddHostedService<PublisherSecurityApp>();
        services.AddHostedService<ConsumerSecurityApp>();
    }
}