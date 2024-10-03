using FluentResults;
using MassTransit;
using MediatR;
using MongoDB.Driver;
using Security.Application.Commands;
using Security.Application.Events;
using Security.Application.Services;
using Security.Infra.Data;
using Security.Infra.Repositories;
using SecurityX.Core.Interfaces;
using SecurityX.Core.Notifications;
using SecurityX.Infra.Cache;
using Sharedx.Infra.Outbox.Cache;
using Sharedx.Infra.Outbox.Services;
using SharedX.Core.Bus;
using SharedX.Core.Entities;
using SharedX.Core.Interfaces;
using SharedX.Core.Specs;
using SharedX.Core.ValueObjects;
using System.Reflection;
namespace SecurityX.ServerApp;
internal class NativeInjectorBoostrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration config)
    {
        services.AddSwaggerGen();

        services.Configure<QueueSettings>(config.GetSection(nameof(QueueSettings)));
        services.Configure<ConnectionZmq>(config.GetSection(nameof(ConnectionZmq)));
        services.Configure<ConnectionRmq>(config.GetSection(nameof(ConnectionRmq)));
        services.Configure<ConnectionRedis>(config.GetSection(nameof(ConnectionRedis)));

        /// Masstransit
        services.AddMassTransit(x =>
        {
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

                cfg.Message<EnvelopeOutbox<SecurityEngine>>(x =>
                {
                    x.SetEntityName("outbox-security-engine");
                });

            });
        });

        // Outbox 
        services.AddSingleton(typeof(IOutboxPublisherService<EnvelopeOutbox<SecurityEngine>>),
                               typeof(OutboxPublisherService<EnvelopeOutbox<SecurityEngine>>));


        // FIX - Application

        //services.AddSingleton<IFixServerApp, FixServerApp>();

        // Domain Bus (Mediator)
        services.AddSingleton<IMediatorHandler, InMemmoryBus>();

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

        //Outbox
        services.AddSingleton(typeof(IOutboxPublisherService<>), typeof(OutboxPublisherService<>));
        services.AddSingleton(typeof(IOutboxCache<>), typeof(OutboxCache<>));

        // Infra - Data
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = new MongoClient(config.GetValue<string>("DatabaseSettings:ConnectionString"));
            return client.GetDatabase(config.GetValue<string>("DatabaseSettings:DatabaseName"));
        });

        // Infra - Context
        services.AddSingleton<ISecurityEngineContext, SecurityEngineContext>();
        services.AddSingleton<ISecurityCache, SecurityCache>();
        services.AddSingleton<ISecurityEngineRepository, SecurityEngineRepository>();

    }
}