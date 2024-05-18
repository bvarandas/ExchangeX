using System.Reflection;
using DropCopyX.Application.Commands;
using DropCopyX.Core.Interfaces;
using DropCopyX.Core.Repositories;
using DropCopyX.Infra.Cache;
using DropCopyX.Infra.Client;
using DropCopyX.Infra.Data;
using DropCopyX.Infra.Repositories;
using DropCopyX.ServerApp.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedX.Core.Bus;
using SharedX.Core.Specs;
namespace DropCopyX.ServerApp;
internal class NativeInjectorBoostrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration config)
    {
        services.AddSwaggerGen();

        services.Configure<QueueCommandSettings>(config.GetSection(nameof(QueueCommandSettings)));
        services.Configure<ConnectionZmq>(config.GetSection(nameof(ConnectionZmq)));

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

        // Domain - Commands
        services.AddSingleton<IRequestHandler<ExecutionReportCommand, bool>, ExecutionReportCommandHandler>();



        // Infra - Data
        services.AddSingleton<IFixSessionCache, FixSessionCache>();
        services.AddSingleton<IExecutedTradeCache, TradeCaptureReportCache>();
        services.AddSingleton<IExecutionReportChache, ExecutionReportChache>();
        services.AddSingleton<IDropCopyContext, DropCopyContext>();
        services.AddSingleton<IDropCopyRepository , DropCopyRepository>();

        services.AddHostedService<ConsumerDropCopyApp>();
        services.AddHostedService<PublisherFixApp>();
    }
}