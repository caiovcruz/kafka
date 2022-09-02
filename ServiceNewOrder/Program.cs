using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Kafka.Producer;
using Serilog;
using ServiceNewOrder;

var build = new ConfigurationBuilder();
build.SetBasePath(Directory.GetCurrentDirectory())
     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
     .AddEnvironmentVariables();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(build.Build())
    .WriteTo.Console()
    .CreateLogger();

var app = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddScoped<IKafkaDispatcher, KafkaDispatcher>();
                    services.AddScoped<INewOrderService, NewOrderService>();
                    services.AddScoped<IOrdersDatabase, OrdersDatabase>();
                })
                .UseSerilog()
                .Build();

app.Run();