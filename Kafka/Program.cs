using Kafka.Producer;
using Kafka.Consumer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

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
                    services.AddScoped<IKafkaService, KafkaService>();
                })
                .UseSerilog()
                .Build();

app.Run();