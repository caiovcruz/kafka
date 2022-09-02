using Confluent.Kafka;
using Kafka.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Kafka.Producer
{
    public class KafkaDispatcher : IKafkaDispatcher
    {
        private readonly ILogger<KafkaDispatcher> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _host;

        public KafkaDispatcher(ILogger<KafkaDispatcher> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _host = _configuration.GetSection("Kafka:Host").Value;
        }

        public async Task<bool> SendMessage<Key, Value>(string topic, CorrelationId id, Key key, Value value)
        {
            var hasMessageBeenSent = false;

            var config = new ProducerConfig { BootstrapServers = _host };

            try
            {

                using (var producer = new ProducerBuilder<string, Message<Value>>(config).SetValueSerializer(new CustomSerializer<Message<Value>>()).Build())
                {
                    try
                    {
                        var keyJson = JsonConvert.SerializeObject(key);
                        var message = new Message<Value>(id, value);

                        await producer.ProduceAsync(topic, new Message<string, Message<Value>>
                        {
                            Key = keyJson,
                            Value = message
                        })
                        .ContinueWith(task =>
                        {
                            if (task.IsFaulted)
                            {
                                _logger.LogError($"{producer.Name} Produce error: {task.Exception?.InnerException}");
                            }
                            else
                            {
                                _logger.LogInformation("======================================================");
                                _logger.LogInformation($"{producer.Name} => Produce {DateTime.Now}");
                                _logger.LogInformation($"Key: {keyJson}");
                                _logger.LogInformation($"Value: {message.Payload}");
                                _logger.LogInformation($"TopicPartitionOffset: {task.Result.TopicPartitionOffset}");

                                hasMessageBeenSent = true;
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"{producer.Name} Produce error: {ex.Message}");
                    }
                }
            }
            catch (Exception xx)
            {
                _logger.LogError($"Produce error: {xx.Message}");
            }

            return hasMessageBeenSent;
        }
    }
}
