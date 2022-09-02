using Database;

namespace ServiceNewOrder
{
    public class OrdersDatabase : IOrdersDatabase
    {
        private LocalDatabase _localDatabase;

        public OrdersDatabase()
        {
            _localDatabase = new LocalDatabase(@$"Data Source=C:\Users\covu\source\repos\Alura\kafka-alura\.NET\Kafka\{GetType().Namespace}\Orders.db");

            _localDatabase.CreateIfNotExists(@"CREATE TABLE Orders (
                                                                    id TEXT NOT NULL UNIQUE,
                                                                    PRIMARY KEY(id)
                                                                   )");
        }

        public async Task<bool> SaveNewAsync(Order order)
        {
            if (await WasProcessedAsync(order.Id))
            {
                return false;
            }

            var affectedRows = await _localDatabase.Command("INSERT INTO Orders (id) VALUES ($id)",
                                                            new Dictionary<string, object> {
                                                                { "$id", order.Id }
                                                            });

            return affectedRows > 0;
        }

        private async Task<bool> WasProcessedAsync(string orderId)
        {
            var order = await _localDatabase.Get<Order>("SELECT id FROM Orders WHERE id = $id",
                                                        new Dictionary<string, object> {
                                                            { "$id", orderId }
                                                        });

            return !string.IsNullOrEmpty(order?.Id);
        }
    }
}
