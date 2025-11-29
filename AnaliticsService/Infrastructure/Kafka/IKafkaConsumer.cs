namespace AnaliticsService.Infrastructure.Kafka;

public interface IKafkaConsumer
{
    Task ConsumeGradeEventsAsync(CancellationToken cancellationToken);
}
