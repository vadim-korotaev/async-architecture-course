using Microsoft.Extensions.Logging;

namespace EventBusRabbitMq;

public class EventBus
{
    private readonly RabbitMqConnection _rabbitMqConnection;
    private readonly ILogger<EventBus> _logger;

    public EventBus(RabbitMqConnection rabbitMqConnection, ILogger<EventBus> logger)
    {
        _rabbitMqConnection = rabbitMqConnection;
        _logger = logger;
    }

    public void Publish(Event @event)
    {
        
    }
}