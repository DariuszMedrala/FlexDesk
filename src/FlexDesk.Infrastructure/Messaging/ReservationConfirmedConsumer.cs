using System.Globalization;
using System.Text;
using System.Text.Json;
using FlexDesk.Application.Common.Email;
using FlexDesk.Application.Common.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FlexDesk.Infrastructure.Messaging;

public class ReservationConfirmedConsumer : BackgroundService
{
    private static readonly string QueueName = nameof(ReservationConfirmedEvent);
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    private readonly RabbitMqConnection _connection;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ReservationConfirmedConsumer> _logger;

    public ReservationConfirmedConsumer(
        RabbitMqConnection connection,
        IEmailSender emailSender,
        ILogger<ReservationConfirmedConsumer> logger)
    {
        _connection = connection;
        _emailSender = emailSender;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "RabbitMQ consumer unavailable, retrying in {Seconds}s", RetryDelay.TotalSeconds);
                try
                {
                    await Task.Delay(RetryDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        var connection = await _connection.GetConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.Span);
                var confirmation = JsonSerializer.Deserialize<ReservationConfirmedEvent>(json);

                if (confirmation is not null)
                {
                    await SendConfirmationEmailAsync(confirmation, stoppingToken);
                }

                await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to process reservation confirmation message");
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        _logger.LogInformation("Listening for reservation confirmations on queue {Queue}", QueueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task SendConfirmationEmailAsync(ReservationConfirmedEvent confirmation, CancellationToken cancellationToken)
    {
        var culture = CultureInfo.InvariantCulture;
        var subject = $"Reservation confirmed: {confirmation.WorkspaceName}";
        var body = string.Create(culture,
            $"Your reservation for {confirmation.WorkspaceName} is confirmed.\n" +
            $"From: {confirmation.StartTime:yyyy-MM-dd HH:mm} UTC\n" +
            $"To:   {confirmation.EndTime:yyyy-MM-dd HH:mm} UTC\n" +
            $"Amount paid: {confirmation.Amount:0.00}\n\n" +
            $"Thank you for using FlexDesk.");

        await _emailSender.SendAsync(confirmation.UserEmail, subject, body, cancellationToken);

        _logger.LogInformation("Sent confirmation email to {Email} for reservation {ReservationId}",
            confirmation.UserEmail, confirmation.ReservationId);
    }
}
