using Confluent.Kafka;
using Kafka.Model;

namespace Kafka.Consumer
{
    public interface IKafkaService
    {
        Task ReceiveMessage<Key, Value, GroupId>(string host, string topic, CancellationToken stoppingToken, Action<ConsumeResult<string, Message<Value>>>? action = null);
    }
}