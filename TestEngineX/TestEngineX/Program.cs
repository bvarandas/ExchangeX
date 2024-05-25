// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedX.Core.Specs;
using System.Reflection;
using TestEngineX;
internal class Program
{
    private static async Task Main(string[] args)
    {
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
                //.UseSerilog(Logging.ConfigureLogger)
                .ConfigureServices(services =>
                {
                    services.Configure<ConnectionZmq>(config.GetSection(nameof(ConnectionZmq)));
                    //services.Configure<QueueCommandSettings>(config.GetSection(nameof(QueueCommandSettings)));
                    //services.AddScoped<ICorrelationIdGenerator, CorrelationIdGenerator>();
                    //services.AddTransient<IMongoDbConnection>((provider) =>
                    //{
                    //    var urlMongo = new MongoDB.Driver.MongoUrl("mongodb://root:example@mongo:27017/challengeCrf?authSource=admin");

                    //    return MongoDbConnection.FromUrl(urlMongo);
                    //});

                    services.AddMediatR(cfg =>
                    {
                        cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());
                    });

                    // Domain - Commands
                    //services.AddSingleton<IRequestHandler<InsertOrderBookCommand, Result<bool>>, InsertOrderBookCommandHandler>();

                    // Infra - Data
                    //services.AddSingleton<IOrderBookRepository, OrderBookRepository>();
                    //services.AddSingleton<IOrderBookContext, OrderBookContext>();
                    services.AddHostedService<ProducerOrderApp>();
                }).Build();

        await host
        .RunAsync();
    }
}