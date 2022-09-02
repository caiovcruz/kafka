using Newtonsoft.Json;

namespace ServiceHttpEcommerce.Model
{
    public class Order
    {
        public decimal Amount { get; set; }
        public string? Email { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
