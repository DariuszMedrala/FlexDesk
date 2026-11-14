using System.Text.Json;
using FlexDesk.Application.Common.Messaging;
using RabbitMQ.Client;

namespace FlexDesk.Infrastructure.Messaging;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly RabbitMqConnection _connection;

    public RabbitMqEventPublisher(RabbitMqConnection connection)
    {
        _connection = connection;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var queue = typeof(TEvent).Name;

        var connection = await _connection.GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(@event);
        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json"
        };

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queue,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}
