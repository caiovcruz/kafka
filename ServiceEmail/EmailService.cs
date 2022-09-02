using Confluent.Kafka;
using Kafka.Consumer;
using Kafka.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ServiceEmail
{
    public class EmailService : BackgroundService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _host;
        private readonly string _topic;
        private IKafkaService _kafkaService;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration, IKafkaService kafkaService)
        {
            _logger = logger;
            _configuration = configuration;
            _host = _configuration.GetSection("Kafka:Host").Value;
            _topic = _configuration.GetSection("Kafka:Topic").Value;
            _kafkaService = kafkaService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _kafkaService.ReceiveMessage<string, string, EmailService>(_host, _topic, stoppingToken, ProcessResult);
            }
        }

        private void ProcessResult(ConsumeResult<string, Message<string>> consumeResult)
        {
            if (consumeResult.Message != null)
            {
                _logger.LogInformation("======================================================");
                _logger.LogInformation($"{typeof(EmailService).Name} => Sendind email at {DateTime.Now}");
                _logger.LogInformation($"Key: {consumeResult.Message.Key}");
                _logger.LogInformation($"Value: {consumeResult.Message.Value.Payload}");
                _logger.LogInformation($"Partition: {consumeResult.Partition}");
                _logger.LogInformation($"Offset: {consumeResult.Offset}");

                Thread.Sleep(1000);

                _logger.LogInformation($"{typeof(EmailService).Name} => Email sent at {DateTime.Now}");
            }
        }
    }
}
