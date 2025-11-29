namespace AnaliticsService.Infrastructure.Kafka.Background;

public class GradeEventsConsumerService : BackgroundService
{
    private readonly IKafkaConsumer _kafkaConsumer;
    private readonly ILogger<GradeEventsConsumerService> _logger;

    public GradeEventsConsumerService(IKafkaConsumer kafkaConsumer,
        ILogger<GradeEventsConsumerService> logger)
    {
        _kafkaConsumer = kafkaConsumer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Grade Events Consumer Service started");
        await _kafkaConsumer.ConsumeGradeEventsAsync(stoppingToken);
        _logger.LogInformation("Grade Events Consumer Service stopped");

    }
}
