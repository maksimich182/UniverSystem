using Confluent.Kafka;
using Infrastructure.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Realisations;

public class KafkaProducer : IKafkaProducer
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:Broker"]
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();

        _logger = logger;
    }


    public async Task ProduceAsync<T>(string topic, T message)
    {
        try
        {
            var messageJson = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<Null, string>
            {
                Value = messageJson
            };

            await _producer.ProduceAsync(topic, kafkaMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to produce message to Kafka topic {topic}");
        }
    }
}
