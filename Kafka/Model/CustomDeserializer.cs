using Confluent.Kafka;
using Newtonsoft.Json;
using System.Text;

namespace Kafka.Model
{
    public class CustomDeserializer<T> : IDeserializer<T>
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            var strings = Encoding.UTF8.GetString(data);

            return JsonConvert.DeserializeObject<T>(strings);
        }
    }
}
