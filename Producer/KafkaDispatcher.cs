using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Producer
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

            using (var producer = new ProducerBuilder<string, string>(config).Build())
            {
                try
                {
                    var message = new Message<Value>(id, value);
                    var keyJson = JsonConvert.SerializeObject(key);
                    var valueJson = JsonConvert.SerializeObject(message);

                    await producer.ProduceAsync(topic, new Message<string, string>
                    {
                        Key = keyJson,
                        Value = valueJson
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
                            _logger.LogInformation($"Value: {valueJson}");
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

            return hasMessageBeenSent;
        }
    }
}
