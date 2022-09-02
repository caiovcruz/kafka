using Microsoft.Extensions.Logging;
using Kafka.Producer;
using Kafka.Model;

namespace ServiceNewOrder
{
    public class NewOrderService : INewOrderService
    {
        private ILogger<NewOrderService> _logger;
        private IKafkaDispatcher _kafkaDispatcher;
        private IOrdersDatabase _ordersDatabase;

        public NewOrderService(ILogger<NewOrderService> logger, IOrdersDatabase ordersDatabase, IKafkaDispatcher kafkaDispatcher)
        {
            _logger = logger;
            _ordersDatabase = ordersDatabase;
            _kafkaDispatcher = kafkaDispatcher;
        }

        public async Task<bool> New(string email, decimal amount)
        {
            var hasOrderBeenSent = false;

            try
            {
                var order = new Order(Guid.NewGuid().ToString(), amount, email);

                if (await _ordersDatabase.SaveNewAsync(order))
                {
                    hasOrderBeenSent = await _kafkaDispatcher.SendMessage("ECOMMERCE_NEW_ORDER", new CorrelationId(GetType().Name), email, order);

                    return hasOrderBeenSent;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{GetType().Name} Error: {ex.Message}");

                return hasOrderBeenSent;
            }

            return hasOrderBeenSent;
        }
    }
}
