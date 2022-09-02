using Confluent.Kafka;
using Newtonsoft.Json;
using System.Text;

namespace Kafka.Model
{
    public class CustomSerializer<T> : ISerializer<T>
    {
        public byte[] Serialize(T data, SerializationContext context)
        {
            var json = JsonConvert.SerializeObject(data);

            return Encoding.UTF8.GetBytes(json);
        }
    }
}
