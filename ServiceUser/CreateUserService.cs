using Confluent.Kafka;
using Database;
using Kafka.Consumer;
using Kafka.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SQLite;

namespace ServiceUser
{
    public class CreateUserService : BackgroundService
    {
        private readonly ILogger<CreateUserService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _host;
        private readonly string _topic;
        private static LocalDatabase _localDatabase;
        private IKafkaService _kafkaService;

        public CreateUserService(ILogger<CreateUserService> logger, IConfiguration configuration, IKafkaService kafkaService)
        {
            _logger = logger;
            _configuration = configuration;
            _host = _configuration.GetSection("Kafka:Host").Value;
            _topic = _configuration.GetSection("Kafka:Topic").Value;
            _kafkaService = kafkaService;

            _localDatabase = new LocalDatabase(@$"Data Source=C:\Users\covu\source\repos\Alura\kafka-alura\.NET\Kafka\{GetType().Namespace}\Users.db");

            _localDatabase.CreateIfNotExists(@"CREATE TABLE Users (
                                                                    id TEXT NOT NULL UNIQUE,
                                                                    email TEXT NOT NULL,
                                                                    PRIMARY KEY(id)
                                                                  )");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(async () =>
            {
                await _kafkaService.ReceiveMessage<string, Order, CreateUserService>(_host, _topic, stoppingToken, ProcessResult);
            });

            return Task.CompletedTask;
        }

        private void ProcessResult(ConsumeResult<string, Message<Order>> consumeResult)
        {
            if (consumeResult.Message != null)
            {
                var order = consumeResult.Message.Value.Payload;

                _logger.LogInformation("======================================================");
                _logger.LogInformation($"{typeof(CreateUserService).Name} => Processing new order, checking for new user at {DateTime.Now}");
                _logger.LogInformation($"Value: {consumeResult.Message.Value}");

                if (!string.IsNullOrEmpty(order?.Email))
                {
                    if (IsNewUserAsync(order.Email).GetAwaiter().GetResult())
                    {
                        InsertNewUser(order.Email);
                    }
                }
            }
        }

        private async Task<bool> IsNewUserAsync(string email)
        {
            try
            {
                var user = await _localDatabase.Get<User>(@"SELECT id AS Id FROM Users WHERE email = $email",
                                                            new Dictionary<string, object>
                                                            {
                                                                { "$email", email }
                                                            });

                if (!string.IsNullOrEmpty(user?.Id))
                {
                    _logger.LogInformation($"{GetType().Name} email: {email} already exists in database!!!");
                }

                return string.IsNullOrEmpty(user?.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{GetType().Name} email: {email} Error: {ex.Message}");
            }

            return false;
        }

        private async void InsertNewUser(string email)
        {
            try
            {
                var affectedRows = await _localDatabase.Command(@"INSERT INTO USERS (id, email) VALUES ($id, $email)",
                                                                new Dictionary<string, object>
                                                                {
                                                                    { "$id", Guid.NewGuid().ToString() },
                                                                    { "$email", email }
                                                                });

                if (affectedRows > 0)
                {
                    _logger.LogInformation($"{GetType().Name} email: {email} inserido");
                }
                else
                {
                    _logger.LogInformation($"{GetType().Name} email: {email} não inserido");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{GetType().Name} email: {email} Error: {ex.Message}");
            }
        }
    }
}
