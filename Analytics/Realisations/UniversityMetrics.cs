using Prometheus;

namespace Analytics.Realisations;

public class UniversityMetrics
{
    private readonly Counter _gradesAddedCounter;
    private readonly Histogram _gradeProcessingDuration;
    private readonly Gauge _kafkaMessagesLag;

    public UniversityMetrics()
    {
        _gradesAddedCounter = Metrics.CreateCounter(
            "university_grades_added_total",
            "Total number of grades added",
            new CounterConfiguration
            {
                LabelNames = new[] { "course_id", "grade_value" }
            });

        _gradeProcessingDuration = Metrics.CreateHistogram(
            "university_grade_processing_duration_seconds",
            "Grade event processing duration",
            new HistogramConfiguration
            {
                LabelNames = new[] { "status" },
                Buckets = new[] { 0.01, 0.05, 0.1, 0.5, 1.0 }
            });

        _kafkaMessagesLag = Metrics.CreateGauge(
            "university_kafka_messages_lag",
            "Kafka messages lag for grade events",
            "consumer_group");
    }

    public void GradeAdded(Guid courseId, int gradeValue)
    {
        _gradesAddedCounter
            .WithLabels(courseId.ToString(), gradeValue.ToString())
            .Inc();
    }

    public void RecordGradeProcessingTime(TimeSpan duration, bool success)
    {
        _gradeProcessingDuration
            .WithLabels(success ? "success" : "error")
            .Observe(duration.TotalSeconds);
    }

    public void SetKafkaLag(long lag)
    {
        _kafkaMessagesLag.Set(lag);
    }
}
