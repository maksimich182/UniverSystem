
using Analytics.Realisations;
using Confluent.Kafka;

namespace AnaliticsService.Infrastructure.Kafka;

public class GradeEventsKafkaConsumer : IKafkaConsumer, IDisposable
{
    private readonly IConsumer<Ignore, string> _consumer;
    //private readonly IGradeAnalyticsService _analyticsSertvice;
    private readonly UniversityMetrics _metrics;
    private readonly ILogger<GradeEventsKafkaConsumer> _logger;

    public GradeEventsKafkaConsumer(IConfiguration configuration,
        //IGradeAnalyticsService analyticsService,
        UniversityMetrics metrics,
        ILogger<GradeEventsKafkaConsumer> logger)
    {
        //_analyticsService = analyticsService;
        _metrics = metrics;
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:Broker"],
            GroupId = "grade-analytics-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config)
            .SetErrorHandler((_, error) =>
                _logger.LogError($"Kafka consumer error: {error.Reason}"))
            .Build();
    }


    public async Task ConsumeGradeEventsAsync(CancellationToken cancellationToken)
    {
        _consumer.Subscribe("grade-events");

        _logger.LogInformation("Grade Events Kafka Consumer started for topic: grade-events");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(cancellationToken);

                if (result != null && result.Topic == "grade-events")
                {
                    await ProcessGradeEventAsync(result.Message.Value);
                    _consumer.Commit(result);

                    _logger.LogDebug("Processed grade event message");
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error processing grade event message");
            }
        }
    }
    private async Task ProcessGradeEventAsync(string message)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
    }
}
