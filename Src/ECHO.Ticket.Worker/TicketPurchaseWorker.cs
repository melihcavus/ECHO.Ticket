using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.DataAccess.Interfaces;

namespace ECHO.Ticket.Worker;

public class TicketPurchaseWorker : BackgroundService
{
    private readonly ILogger<TicketPurchaseWorker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public TicketPurchaseWorker(ILogger<TicketPurchaseWorker> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
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

                        var ticket = await ticketRepo.GetByIdAsync(purchaseData.TicketId);

                        if (ticket != null && ticket.Capacity > 0)
                        {
                            ticket.Capacity--;
                            ticketRepo.Update(ticket);

                            var newPledge = new Pledge
                            {
                                Id = Guid.NewGuid(),
                                UserId = purchaseData.UserId,
                                TicketId = purchaseData.TicketId,
                                PledgeDate = DateTime.UtcNow,
                                AmountPaid = ticket.Price 
                            };
                            await pledgeRepo.AddAsync(newPledge);

                            await ticketRepo.SaveChangesAsync();

                            _logger.LogInformation($"[v] İşlem Başarılı: Ticket ID {ticket.Id} için stok düşüldü. Kullanıcı {purchaseData.UserId} için destek kaydı oluşturuldu.");
                        }
                        else
                        {
                            _logger.LogWarning($"[x] İşlem Başarısız: Bilet bulunamadı veya tükenmiş! (Ticket ID: {purchaseData.TicketId})");
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