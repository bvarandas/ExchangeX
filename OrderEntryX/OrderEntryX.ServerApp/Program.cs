using Common.logging;
using OrderEntryX.ServerApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
.Build();

IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(builder =>
        {
            builder.Sources.Clear();
            builder.AddConfiguration(config);

        })
        .UseSerilog(Logging.ConfigureLogger)
        .ConfigureServices(services =>
        {
            NativeInjectorBoostrapper.RegisterServices(services, config);
        }).Build();

await host
.RunAsync();