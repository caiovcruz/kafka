using Confluent.Kafka;
using Kafka.Model;
using Kafka.Producer;
using Microsoft.Extensions.Logging;

namespace Kafka.Consumer
{
    public class KafkaService : IKafkaService
    {
        private readonly ILogger<KafkaService> _logger;
        private readonly IKafkaDispatcher _kafkaDispatcher;

        public KafkaService(ILogger<KafkaService> logger, IKafkaDispatcher kafkaDispatcher)
        {
            _logger = logger;
            _kafkaDispatcher = kafkaDispatcher;
        }

        public Task ReceiveMessage<Key, Value, GroupId>(string host, string topic, CancellationToken stoppingToken, Action<ConsumeResult<string, Message<Value>>>? action = null)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = host,
                GroupId = typeof(GroupId).Name,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using (var consumer = new ConsumerBuilder<string, Message<Value>>(config).SetValueDeserializer(new CustomDeserializer<Message<Value>>()).Build())
            {
                consumer.Subscribe(topic);

                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume();

                            if (consumeResult.Message != null)
                            {
                                try
                                {
                                    if (action != null)
                                    {
                                        action?.Invoke(consumeResult);
                                    }
                                    else
                                    {
                                        LogResult(config.GroupId, consumeResult);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"{config.GroupId} Consume error: {ex.Message}");

                                    _kafkaDispatcher.SendMessage("ECOMMERCE_DEADLETTER",
                                        consumeResult.Message.Value.Id.ContinueWith("DeadLetter"),
                                        consumeResult.Message.Value.Id.ToString(),
                                        consumeResult);
                                }
                            }
                        }
                        catch (ConsumeException e)
                        {
                            _logger.LogError($"{config.GroupId} Consume error: {e.Error.Reason}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    consumer.Close();
                }
            }

            return Task.CompletedTask;
        }

        private void LogResult<Value>(string groupId, ConsumeResult<string, Message<Value>> consumeResult)
        {
            _logger.LogInformation("======================================================");
            _logger.LogInformation($"{groupId} => Consumed {DateTime.Now}");
            _logger.LogInformation($"Key: {consumeResult.Message.Key}");
            _logger.LogInformation($"Value: {consumeResult.Message.Value}");
            _logger.LogInformation($"Partition: {consumeResult.Partition}");
            _logger.LogInformation($"Offset: {consumeResult.Offset}");
        }
    }
}
