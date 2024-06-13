using System.Reflection;
using DropCopyX.Application.Commands;
using DropCopyX.Core.Interfaces;
using DropCopyX.Core.Repositories;
using DropCopyX.Infra.Cache;
using DropCopyX.Infra.Client;
using DropCopyX.Infra.Data;
using DropCopyX.Infra.Repositories;
using DropCopyX.ServerApp.Services;
using FluentResults;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sharedx.Infra.Outbox.Cache;
using Sharedx.Infra.Outbox.Services;
using SharedX.Core.Bus;
using SharedX.Core.Interfaces;
using SharedX.Core.Specs;
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
            x.AddConsumer<DropCopyOutboxApp>();
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

        // Outbox 
        services.AddSingleton(typeof(IOutboxBackgroundService<>), typeof(OutboxBackgroundService<>));
        services.AddSingleton(typeof(IOutboxCache<>), typeof(OutboxCache<>));

        // Domain - Commands
        services.AddSingleton<IRequestHandler<ExecutionReportCommand, Result>, ExecutionReportCommandHandler>();

        // Infra - Data
        services.AddSingleton<IFixSessionDropCopyCache, FixSessionDropCopyCache>();
        services.AddSingleton<IExecutedTradeCache, TradeCaptureReportCache>();
        services.AddSingleton<IExecutionReportChache, ExecutionReportChache>();
        services.AddSingleton<IDropCopyContext, DropCopyContext>();
        services.AddSingleton<IDropCopyRepository , DropCopyRepository>();

        services.AddHostedService<ConsumerDropCopyApp>();
        services.AddHostedService<PublisherFixApp>();
    }
}