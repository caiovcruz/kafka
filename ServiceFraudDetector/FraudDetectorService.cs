using Confluent.Kafka;
using Kafka.Consumer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Kafka.Producer;
using Kafka.Model;
using Database;

namespace ServiceFraudDetector
{
    public class FraudDetectorService : BackgroundService
    {
        private readonly ILogger<FraudDetectorService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _host;
        private readonly string _topic;
        private IKafkaService _kafkaService;
        private IKafkaDispatcher _kafkaDispatcher;
        private LocalDatabase _localDatabase;

        public FraudDetectorService(ILogger<FraudDetectorService> logger,
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

            _localDatabase = new LocalDatabase(@$"Data Source=C:\Users\covu\source\repos\Alura\kafka-alura\.NET\Kafka\{GetType().Namespace}\Orders.db");

            _localDatabase.CreateIfNotExists(@"CREATE TABLE Orders (
                                                                    id TEXT NOT NULL UNIQUE,
                                                                    is_fraud INTEGER NOT NULL,
                                                                    PRIMARY KEY(id)
                                                                   )");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _kafkaService.ReceiveMessage<string, Order, FraudDetectorService>(_host, _topic, stoppingToken, ProcessResult);
            }
        }

        private async void ProcessResult(ConsumeResult<string, Message<Order>> consumeResult)
        {
            if (consumeResult.Message != null)
            {
                var order = consumeResult.Message.Value.Payload;

                _logger.LogInformation("======================================================");
                _logger.LogInformation($"{typeof(FraudDetectorService).Name} => Processing new order, checking for fraud at {DateTime.Now}");
                _logger.LogInformation($"Key: {consumeResult.Message.Key}");
                _logger.LogInformation($"Value: {consumeResult.Message.Value.Payload}");
                _logger.LogInformation($"Partition: {consumeResult.Partition}");
                _logger.LogInformation($"Offset: {consumeResult.Offset}");

                if (WasProcessedAsync(order.Id).GetAwaiter().GetResult())
                {
                    _logger.LogInformation($"Order {order.Id} was already processed!");
                    return;
                }

                Thread.Sleep(5000);

                // pretending that the fraud happens when the amount is >= 4500
                var orderStatus = IsFraud(order);

                var affectedRows = await _localDatabase.Command("INSERT INTO Orders (id, is_fraud) VALUES ($id, $is_fraud)",
                                                                new Dictionary<string, object>
                                                                {
                                                                    { "$id", order.Id },
                                                                    { "$is_fraud", orderStatus.Item2 }
                                                                });

                _logger.LogInformation($"Order is {orderStatus.Item1}: {consumeResult.Message.Value.Payload}");
                await _kafkaDispatcher.SendMessage($"ECOMMERCE_{orderStatus.Item1}_ORDER",
                                                    consumeResult.Message.Value.Id.ContinueWith(GetType().Name),
                                                    order?.Email,
                                                    order);
            }
        }

        private static (string, bool) IsFraud(Order response)
        {
            return response?.Amount >= 4500
                                    ? ("REJECTED", true)
                                    : ("APPROVED", false);
        }

        private async Task<bool> WasProcessedAsync(string orderId)
        {
            var order = await _localDatabase.Get<Order>("SELECT id FROM Orders WHERE id = $id",
                                                            new Dictionary<string, object> {
                                                                { "$id", orderId }
                                                            });
            return !string.IsNullOrEmpty(order?.Id);
        }
    }
}
