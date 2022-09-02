using Confluent.Kafka;
using Consumer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ServiceEmail
{
    public class LogService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly string _host;
        private IKafkaService _kafkaService;

        public LogService(IConfiguration configuration, IKafkaService kafkaService)
        {
            _configuration = configuration;
            _host = _configuration.GetSection("Kafka:Host").Value;
            _kafkaService = kafkaService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var adminConfig = new AdminClientConfig()
            {
                BootstrapServers = _host
            };

            using (var adminClient = new AdminClientBuilder(adminConfig).Build())
            {
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
                var topics = metadata.Topics.Select(a => a.Topic).ToList();

                _kafkaService.ReceiveMessage<string, string, LogService>(_host, topics, stoppingToken);
            }

            return Task.CompletedTask;
        }
    }
}
