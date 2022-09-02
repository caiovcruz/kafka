using CommandLine;
using Consumer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ServiceEmail;

var build = new ConfigurationBuilder();
build.SetBasePath(Directory.GetCurrentDirectory())
     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
     .AddEnvironmentVariables();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(build.Build())
    .WriteTo.Console()
    .CreateLogger();

Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    var app = Host.CreateDefaultBuilder()
                                    .ConfigureServices((context, services) =>
                                    {
                                        services.AddScoped<IKafkaService, KafkaService>();
                                        services.AddHostedService<LogService>();
                                    })
                                    .UseSerilog()
                                    .Build();

                    app.Run();
                });