using Common.logging;
using SecurityX.ServerApp;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

// Add services to the container.
builder.Services.AddControllers();

NativeInjectorBoostrapper.RegisterServices(builder.Services, config);
builder.Host.UseSerilog(Logging.ConfigureLogger);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseMvc();
app.UseAuthorization();

app.MapControllers();

app.Run();
