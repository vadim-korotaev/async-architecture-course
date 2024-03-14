using EventBusRabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Common.Extensions;

public static class RabbitMqExtension
{
    public static IServiceCollection AddRabbitMqConnection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<RabbitMqConnection>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMqConnection>>();

            var factory = new ConnectionFactory()
            {
                HostName = configuration["EventBusConnection"],
                DispatchConsumersAsync = true
            };

            if (!string.IsNullOrEmpty(configuration["EventBusUserName"]))
            {
                factory.UserName = configuration["EventBusUserName"];
            }

            if (!string.IsNullOrEmpty(configuration["EventBusPassword"]))
            {
                factory.Password = configuration["EventBusPassword"];
            }

            return new RabbitMqConnection(factory, logger);
        });

        return services;
    }

    public static IServiceCollection AddProducer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<RabbitMqProducer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMqProducer>>();
            var connection = sp.GetRequiredService<RabbitMqConnection>();
            var exchangeName = configuration["ExchangeName"];

            return new RabbitMqProducer(connection, logger, exchangeName);
        });

        return services;
    }
    
}