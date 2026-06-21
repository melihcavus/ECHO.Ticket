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

namespace ECHO.Ticket.Worker;

public class TicketPurchaseWorker : BackgroundService
{
    private readonly ILogger<TicketPurchaseWorker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;

    public TicketPurchaseWorker(ILogger<TicketPurchaseWorker> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rabbitHost = _configuration["RabbitMQ:HostName"] ?? "localhost";
        var factory = new ConnectionFactory { HostName = rabbitHost };
        
        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        string queueName = "ticket_purchase_queue";
        await channel.QueueDeclareAsync(queue: queueName,
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null, 
                                        cancellationToken: stoppingToken);

        _logger.LogInformation($"[*] {queueName} kuyruğu dinleniyor...");

        var consumer = new AsyncEventingBasicConsumer(channel);
        
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            _logger.LogInformation($"[x] Yeni bilet talebi alındı: {message}");

            try
            {
                var purchaseData = JsonSerializer.Deserialize<TicketPurchaseMessageDto>(message);

                if (purchaseData != null)
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
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
                                    await client.PostAsJsonAsync($"{apiUrl}/api/tickets/broadcast-seat", payload, stoppingToken);
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
                }

                await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[x] Mesaj işlenirken bir hata oluştu: {ex.Message}");
                await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}