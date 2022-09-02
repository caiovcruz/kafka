using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common_kafka
{
    public interface ConsumerFunction<T>
    {
        void Consume(ConsumerRecord<string, T> record);
    }
}
