
using AnaliticsService.Services;
using Analytics.Realisations;
using Confluent.Kafka;
using Infrastructure.Events;
using System.Text.Json;

namespace AnaliticsService.Infrastructure.Kafka;

public class GradeEventsKafkaConsumer : IKafkaConsumer, IDisposable
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly UniversityMetrics _metrics;
    private readonly ILogger<GradeEventsKafkaConsumer> _logger;

    public GradeEventsKafkaConsumer(IConfiguration configuration,
        IServiceProvider serviceProvider,
        UniversityMetrics metrics,
        ILogger<GradeEventsKafkaConsumer> logger)
    {
        _serviceProvider = serviceProvider;
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
                    // Для каждого сообщения создаем новый scope
                    using var scope = _serviceProvider.CreateScope();
                    var analyticsService = scope.ServiceProvider.GetRequiredService<IGradeAnalyticsService>();

                    await ProcessGradeEventAsync(result.Message.Value, analyticsService);
                    _consumer.Commit(result);

                    _logger.LogDebug("Processed grade event message");
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing grade event message");
            }
        }
    }

    public void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
    }

    private async Task ProcessGradeEventAsync(string message, IGradeAnalyticsService analyticsService)
    {
        try
        {
            var gradeEvent = JsonSerializer.Deserialize<GradeAddedEvent>(message);
            if (gradeEvent != null)
            {
                await analyticsService.ProcessGradeEventAsync(gradeEvent);
                _logger.LogInformation($"Successfully processed grade event for student: {gradeEvent.StudentId}");
            }
            else
            {
                _logger.LogWarning($"Failed to deserialize grade event message: {message}");
            }
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, $"JSON deserialization error for grade event message: {message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing grade event message: {message}");
        }
    }
}

