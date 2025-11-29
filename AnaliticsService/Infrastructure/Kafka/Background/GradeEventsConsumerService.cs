namespace AnaliticsService.Infrastructure.Kafka.Background;

public class GradeEventsConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GradeEventsConsumerService> _logger;

    public GradeEventsConsumerService(IServiceProvider serviceProvider,
        ILogger<GradeEventsConsumerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Grade Events Consumer Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Создаем scope для каждого цикла потребления
                using var scope = _serviceProvider.CreateScope();
                var kafkaConsumer = scope.ServiceProvider.GetRequiredService<IKafkaConsumer>();

                await kafkaConsumer.ConsumeGradeEventsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Grade Events Consumer Service");
                // Пауза перед повторной попыткой
                await Task.Delay(5000, stoppingToken);
            }
        }

        _logger.LogInformation("Grade Events Consumer Service stopped");
    }
}
