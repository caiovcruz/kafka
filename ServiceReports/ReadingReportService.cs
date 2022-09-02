using Microsoft.Extensions.Logging;
using Kafka.Consumer;
using Kafka.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace ServiceReports
{
    public class ReadingReportService : BackgroundService
    {
        private ILogger<ReadingReportService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _host;
        private readonly string _topic;
        private IKafkaService _kafkaService;

        private const string REPORTS_PATH = @"C:\Users\covu\source\repos\Alura\kafka-alura\.NET\Kafka\ServiceReports\Reports";
        private const string BASE_FILE = @"C:\Users\covu\source\repos\Alura\kafka-alura\.NET\Kafka\ServiceReports\report.txt";


        public ReadingReportService(ILogger<ReadingReportService> logger, IConfiguration configuration, IKafkaService kafkaService)
        {
            _logger = logger;
            _configuration = configuration;
            _host = _configuration.GetSection("Kafka:Host").Value;
            _topic = _configuration.GetSection("Kafka:Topic").Value;
            _kafkaService = kafkaService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _kafkaService.ReceiveMessage<string, User, ReadingReportService>(_host, _topic, stoppingToken, ProcessResult);
            }
        }

        private void ProcessResult(ConsumeResult<string, Message<User>> consumeResult)
        {
            if (consumeResult.Message != null)
            {
                var user = consumeResult.Message.Value.Payload;

                _logger.LogInformation("======================================================");
                _logger.LogInformation($"{GetType().Name} => Processing report for {consumeResult.Message.Value} at {DateTime.Now}");

                if (!Directory.Exists(REPORTS_PATH))
                {
                    Directory.CreateDirectory(REPORTS_PATH);
                }

                var reportFile = Path.Combine(REPORTS_PATH, user?.GetReportFileName() ?? $"error-{Guid.NewGuid()}-report.txt");

                File.Copy(BASE_FILE, reportFile, true);

                using (var sw = new StreamWriter(reportFile, append: true))
                {
                    sw.WriteLineAsync($"Create for {user?.Id}");
                }

                _logger.LogInformation($"{GetType().Name} => Report processed at {DateTime.Now}");
                _logger.LogInformation($"File created: {Path.GetFullPath(reportFile)}");
            }
        }
    }
}
