using Confluent.Kafka;
using Database;
using Kafka.Consumer;
using Kafka.Model;
using Kafka.Producer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data.SQLite;

namespace ServiceUser
{
    public class BatchSendMessageService : BackgroundService
    {
        private readonly ILogger<BatchSendMessageService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _host;
        private readonly string _topic;
        private LocalDatabase _localDatabase;
        private IKafkaService _kafkaService;
        private IKafkaDispatcher _kafkaDispatcher;

        public BatchSendMessageService(ILogger<BatchSendMessageService> logger,
                                       IConfiguration configuration,
                                       IKafkaService kafkaService,
                                       IKafkaDispatcher kafkaDispatcher)
        {
            _logger = logger;
            _configuration = configuration;
            _host = _configuration.GetSection("Kafka:Host").Value;
            _topic = _configuration.GetSection("Kafka:BatchTopic").Value;
            _kafkaService = kafkaService;
            _kafkaDispatcher = kafkaDispatcher;

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
                await _kafkaService.ReceiveMessage<string, string, BatchSendMessageService>(_host, _topic, stoppingToken, ProcessResult);
            });

            return Task.CompletedTask;
        }

        private void ProcessResult(ConsumeResult<string, Message<string>> consumeResult)
        {
            if (consumeResult.Message != null)
            {
                _logger.LogInformation("======================================================");
                _logger.LogInformation($"{GetType().Name} => Processing new batch at {DateTime.Now}");
                _logger.LogInformation($"Topic: {consumeResult.Message.Value.Payload}");

                var users = GetAllUsersAsync().GetAwaiter().GetResult();

                if (users?.Count > 0)
                {
                    foreach (var user in users)
                    {
                        _kafkaDispatcher.SendMessage(consumeResult.Message.Value.Payload,
                                                     consumeResult.Message.Value.Id.ContinueWith(GetType().Name),
                                                     user.Id,
                                                     user);

                        _logger.LogInformation($"Acho que enviei para: {user.Id}");
                    }
                }
            }
        }

        private async Task<List<User>?> GetAllUsersAsync()
        {
            try
            {
                return await _localDatabase.GetAll<User>(@"SELECT id AS Id FROM Users");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{GetType().Name} Error: {ex.Message}");
            }

            return default;
        }
    }
}
