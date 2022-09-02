using Kafka.Model;

namespace Kafka.Producer
{
    public interface IKafkaDispatcher
    {
        Task<bool> SendMessage<Key, Value>(string topic, CorrelationId id, Key key, Value value);
    }
}