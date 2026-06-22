using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECHO.Ticket.Business.RabbitMQ;

public class RabbitMQProducer : IMessageProducer, IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQProducer> _logger;
    private IConnection? _connection;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public RabbitMQProducer(IConfiguration configuration, ILogger<RabbitMQProducer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private async Task<IConnection> GetOrCreateConnectionAsync()
    {
        if (_connection is { IsOpen: true })
            return _connection;

        await _semaphore.WaitAsync();
        try
        {
            if (_connection is { IsOpen: true })
                return _connection;

            // ConnectionString üzerinden okuyarak appsettings ve Render ile tam uyum sağlıyoruz
            var uri = _configuration.GetConnectionString("RabbitMQConnection") 
                      ?? "amqp://guest:guest@localhost:5672/";

            var factory = new ConnectionFactory
            {
                Uri = new Uri(uri),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                RequestedHeartbeat = TimeSpan.FromSeconds(60),
            };

            _connection = await factory.CreateConnectionAsync("ECHO-API-Producer");
            _logger.LogInformation("RabbitMQ bağlantısı başarıyla kuruldu.");

            return _connection;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SendMessageAsync<T>(T message, string queueName)
    {
        try
        {
            var connection = await GetOrCreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var props = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json"
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: false,
                basicProperties: props,
                body: body);

            _logger.LogInformation("Mesaj kuyruğa gönderildi. Queue: {QueueName}", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ mesaj gönderimi başarısız. Queue: {QueueName}", queueName);
            _connection = null; // Hata durumunda bağlantıyı sıfırlıyoruz ki tekrar denesin
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
        _semaphore.Dispose();
    }
}