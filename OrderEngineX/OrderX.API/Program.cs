using Common.logging;
using OrderEngineX.API.Config;
using Serilog;
using SharedX.Core.Specs;

Console.WriteLine("Hello, World!");
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
            services.Configure<ConnectionZmq>(config.GetSection(nameof(ConnectionZmq)));
            NativeInjectorBoostrapper.RegisterServices(services, config);
        }).Build();

await host
.RunAsync();