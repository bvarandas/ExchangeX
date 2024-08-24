using DropCopyX.Application.Commands;
using DropCopyX.Core.Interfaces;
using DropCopyX.Core.Repositories;
using DropCopyX.Infra.Cache;
using DropCopyX.Infra.Data;
using DropCopyX.Infra.Repositories;
using DropCopyX.ServerApp.Receiver;
using DropCopyX.ServerApp.Services;
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

        services.AddMassTransit(x =>
        {
            x.AddConsumer(typeof(IOutboxConsumerService<>), typeof(OutboxConsumerService<>));
            //x.AddConsumer<DropCopyOutboxApp>();
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
        services.AddSingleton(typeof(ReceiverDropCopy), typeof(IReceiverEngine<ExecutionReport>));

        // Outbox 
        services.AddSingleton(typeof(IOutboxConsumerService<EnvelopeOutbox<ExecutionReport>>),
                               typeof(OutboxConsumerService<EnvelopeOutbox<ExecutionReport>>));

        services.AddSingleton(typeof(IOutboxCache<>), typeof(OutboxCache<>));

        // Domain - Commands
        services.AddSingleton<IRequestHandler<ExecutionReportCommand, Result>, ExecutionReportCommandHandler>();

        // Infra - Data
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = new MongoClient(config.GetValue<string>("DatabaseSettings:ConnectionString"));
            return client.GetDatabase(config.GetValue<string>("DatabaseSettings:DatabaseName"));
        });

        services.AddSingleton<IFixSessionDropCopyCache, FixSessionDropCopyCache>();
        services.AddSingleton<IExecutedTradeCache, TradeCaptureReportCache>();
        services.AddSingleton<IExecutionReportChache, ExecutionReportChache>();
        services.AddSingleton<IDropCopyContext, DropCopyContext>();
        services.AddSingleton<IDropCopyRepository, DropCopyRepository>();

        services.AddHostedService<PublisherFixApp>();
    }
}