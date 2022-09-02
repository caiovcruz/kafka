using Newtonsoft.Json;

namespace Kafka.Model
{
    public class CorrelationId
    {
        public string Id { get; set; }

        public CorrelationId(string title)
        {
            Id = $"{title}({Guid.NewGuid()})";
        }

        public CorrelationId ContinueWith(string title)
        {
            return new CorrelationId($"{Id}-{title}");
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
