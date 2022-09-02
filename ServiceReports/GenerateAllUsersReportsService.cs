using Microsoft.Extensions.Logging;
using Kafka.Producer;
using Kafka.Model;

namespace ServiceReports
{
    public class GenerateAllUsersReportsService : IGenerateAllUsersReportsService
    {
        private ILogger<GenerateAllUsersReportsService> _logger;
        private IKafkaDispatcher _kafkaDispatcher;

        public GenerateAllUsersReportsService(ILogger<GenerateAllUsersReportsService> logger, IKafkaDispatcher kafkaDispatcher)
        {
            _logger = logger;
            _kafkaDispatcher = kafkaDispatcher;
        }

        public async Task<bool> Run()
        {
            try
            {
                return await _kafkaDispatcher.SendMessage("ECOMMERCE_SEND_MESSAGE_TO_ALL_USERS",
                                                          new CorrelationId(GetType().Name),
                                                          "ECOMMERCE_USER_GENERATE_READING_REPORT",
                                                          "ECOMMERCE_USER_GENERATE_READING_REPORT");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{GetType().Name} Produce error: {ex.Message}");

                return false;
            }
        }
    }
}
