using Confluent.Kafka;
using Kafka.Consumer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Kafka.Producer;
using Kafka.Model;

namespace ServiceFraudDetector
{
    public class EmailNewOrderService : BackgroundService
    {
        private readonly ILogger<EmailNewOrderService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _host;
        private readonly string _topic;
        private IKafkaService _kafkaService;
        private IKafkaDispatcher _kafkaDispatcher;

        public EmailNewOrderService(ILogger<EmailNewOrderService> logger,
                                    IConfiguration configuration,
                                    IKafkaService kafkaService,
                                    IKafkaDispatcher kafkaDispatcher)
        {
            _logger = logger;
            _configuration = configuration;
            _host = _configuration.GetSection("Kafka:Host").Value;
            _topic = _configuration.GetSection("Kafka:Topic").Value;
            _kafkaService = kafkaService;
            _kafkaDispatcher = kafkaDispatcher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _kafkaService.ReceiveMessage<string, Order, EmailNewOrderService>(_host, _topic, stoppingToken, ProcessResult);
            }
        }

        private void ProcessResult(ConsumeResult<string, Message<Order>> consumeResult)
        {
            if (consumeResult.Message != null)
            {
                _logger.LogInformation("======================================================");
                _logger.LogInformation($"{typeof(EmailNewOrderService).Name} => Processing new order, preparing email at {DateTime.Now}");
                _logger.LogInformation($"Value: {consumeResult.Message.Value.Payload}");

                var emailCode = "Thank you for your order! We are processing your order!";
                _kafkaDispatcher.SendMessage("ECOMMERCE_SEND_EMAIL",
                                             consumeResult.Message.Value.Id.ContinueWith(GetType().Name),
                                             consumeResult.Message.Value.Payload.Email,
                                             emailCode)
                                .GetAwaiter()
                                .GetResult();
            }
        }
    }
}
