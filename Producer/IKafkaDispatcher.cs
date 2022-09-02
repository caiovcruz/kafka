namespace Producer
{
    public interface IKafkaDispatcher
    {
        Task<bool> SendMessage<Key, Value>(string topic, Key key, CorrelationId id, Value value);
    }
}