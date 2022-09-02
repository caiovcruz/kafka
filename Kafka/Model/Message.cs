using Newtonsoft.Json;

namespace Kafka.Model
{
    public class Message<T>
    {
        public Message(CorrelationId id, T payload)
        {
            Id = id;
            Payload = payload;
        }

        public CorrelationId Id { get; set; }
        public T Payload { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
