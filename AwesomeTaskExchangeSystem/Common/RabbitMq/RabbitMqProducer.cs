using System.Text;
using Common.Events;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace EventBusRabbitMq;

public class RabbitMqProducer
{
    private readonly RabbitMqConnection _connection;
    private readonly ILogger<RabbitMqProducer> _logger;
    private readonly string _exchangeName;

    public RabbitMqProducer(RabbitMqConnection connection, ILogger<RabbitMqProducer> logger, string exchangeName)
    {
        _connection = connection;
        _logger = logger;
        _exchangeName = exchangeName;
    }

    public void Publish(Event @event)
    {
        using var channel = _connection.CreateModel();

        channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct);
        
        var message = JsonConvert.SerializeObject(@event);
        var body = Encoding.UTF8.GetBytes(message);
        
        var eventName = @event.GetType().Name;
        
        channel.BasicPublish(
            exchange: _exchangeName,
            routingKey: eventName,
            mandatory: true,
            basicProperties: null,
            body: body);
    }
}