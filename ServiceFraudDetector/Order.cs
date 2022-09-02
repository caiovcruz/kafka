using Newtonsoft.Json;

namespace ServiceFraudDetector
{
    public class Order
    {
        public Order(string id, decimal amount, string email)
        {
            Id = id;
            Amount = amount;
            Email = email;
        }

        public string Id { get; set; }
        public decimal Amount { get; set; }
        public string Email { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
