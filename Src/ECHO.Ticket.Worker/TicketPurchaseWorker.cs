using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json; 
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Configuration;
using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.DataAccess.Interfaces;
using System.Net.Http;

namespace ECHO.Ticket.Worker;

public class TicketPurchaseWorker : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TicketPurchaseWorker> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    private const string QueueName = "ticket_purchase_queue";

    public TicketPurchaseWorker(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILogger<TicketPurchaseWorker> logger)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await WaitForRabbitMQAsync(stoppingToken);
        await SetupConsumerAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task WaitForRabbitMQAsync(CancellationToken ct)
    {
        var uri = _configuration.GetConnectionString("RabbitMQConnection") 
                  ?? "amqp://guest:guest@localhost:5672/";

        var factory = new ConnectionFactory
        {
            Uri = new Uri(uri),
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            RequestedHeartbeat = TimeSpan.FromSeconds(60),
        };

        var attempt = 0;
        while (!ct.IsCancellationRequested)
        {
            attempt++;
            try
            {
                _logger.LogInformation("RabbitMQ bağlantısı deneniyor (Deneme #{Attempt})...", attempt);
                _connection = await factory.CreateConnectionAsync("ECHO-Worker-Consumer", ct);
                _channel = await _connection.CreateChannelAsync(cancellationToken: ct);
                _logger.LogInformation("RabbitMQ bağlantısı başarılı.");
                return;
            }
            catch (Exception ex)
            {
                var delay = TimeSpan.FromSeconds(Math.Min(attempt * 5, 60)); 
                _logger.LogWarning(ex, "RabbitMQ bağlantısı kurulamadı. {Delay} sonra tekrar denenecek.", delay);
                await Task.Delay(delay, ct);
            }
        }
    }

    private async Task SetupConsumerAsync(CancellationToken ct)
    {
        if (_channel == null) return;

        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct);

        // Sistemin boğulmasını engellemek için mesajları tek tek çekiyoruz (QoS)
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, ct);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Mesaj alındı: {Message}", json);

            try
            {
                var purchaseData = JsonSerializer.Deserialize<TicketPurchaseMessageDto>(json);
                if (purchaseData != null)
                {
                    await ProcessPurchaseAsync(purchaseData, ct);
                }
                
                await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mesaj işlenirken hata oluştu.");
                await _channel.BasicNackAsync(eventArgs.DeliveryTag, false, requeue: false);
            }
        };

        await _channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer, cancellationToken: ct);
        _logger.LogInformation("Worker kuyruk dinlemesi başladı. Queue: {QueueName}", QueueName);
    }

    private async Task ProcessPurchaseAsync(TicketPurchaseMessageDto purchaseData, CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var ticketRepo = scope.ServiceProvider.GetRequiredService<IRepository<Core.Entities.Ticket>>();
        var pledgeRepo = scope.ServiceProvider.GetRequiredService<IRepository<Pledge>>();
        var userRepo = scope.ServiceProvider.GetRequiredService<IRepository<ECHO.Ticket.Core.Entities.User>>();

        var ticket = await ticketRepo.GetByIdAsync(purchaseData.TicketId);
        var user = await userRepo.GetByIdAsync(purchaseData.UserId);

        if (ticket != null && ticket.Capacity > 0 && user != null)
        {
            if (user.Balance >= ticket.Price)
            {
                user.Balance -= ticket.Price;
                userRepo.Update(user);

                ticket.Capacity--;
                ticketRepo.Update(ticket);

                var newPledge = new Pledge
                {
                    Id = Guid.NewGuid(),
                    UserId = purchaseData.UserId,
                    TicketId = purchaseData.TicketId,
                    PledgeDate = DateTime.UtcNow,
                    AmountPaid = ticket.Price,
                    RowLabel = purchaseData.RowLabel, 
                    ColumnNumber = purchaseData.ColumnNumber 
                };
                await pledgeRepo.AddAsync(newPledge);

                await ticketRepo.SaveChangesAsync();
                await userRepo.SaveChangesAsync();
                await pledgeRepo.SaveChangesAsync();

                _logger.LogInformation($"[v] Islem Basarili: Ticket ID {ticket.Id}. Kullanici {user.Id} bakiyesinden {ticket.Price} dusuldu.");

                if (!string.IsNullOrEmpty(purchaseData.RowLabel) && purchaseData.ColumnNumber.HasValue)
                {
                    using var client = new HttpClient();
                    var payload = new SeatSoldEventDto 
                    { 
                        EventId = purchaseData.EventId, 
                        SeatLabel = $"{purchaseData.RowLabel}-{purchaseData.ColumnNumber}" 
                    };
                    
                    var apiUrl = _configuration["ApiBaseUrl"] ?? "http://localhost:5216";
                    await client.PostAsJsonAsync($"{apiUrl}/api/tickets/broadcast-seat", payload, ct);
                }
            }
            else
            {
                _logger.LogWarning($"[x] Islem Basarisiz: Yetersiz Bakiye! (Kullanici: {user.Id})");
            }
        }
        else
        {
            _logger.LogWarning($"[x] Islem Basarisiz: Bilet bulunamadi veya tukenmis! (Ticket ID: {purchaseData.TicketId})");
        }
    }

    public override async Task StopAsync(CancellationToken ct)
    {
        _logger.LogInformation("Worker durduruluyor...");
        if (_channel != null) await _channel.DisposeAsync();
        if (_connection != null) await _connection.DisposeAsync();
        await base.StopAsync(ct);
    }
}