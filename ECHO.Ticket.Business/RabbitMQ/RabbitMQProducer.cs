using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ECHO.Ticket.Business.RabbitMQ;

public class RabbitMQProducer : IMessageProducer
{
    private readonly IConfiguration _configuration;

    public RabbitMQProducer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendMessageAsync<T>(T message, string queueName)
    {
        // 1. SİHİRLİ DEĞİŞİKLİK: Adresi ConnectionString'den çekiyoruz
        var connectionString = _configuration.GetConnectionString("RabbitMQConnection") 
                               ?? "amqp://guest:guest@localhost:5672/";

        // 2. SİHİRLİ DEĞİŞİKLİK: HostName yerine Uri kullanıyoruz
        var factory = new ConnectionFactory 
        { 
            Uri = new Uri(connectionString) 
        };
        
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(exchange: string.Empty,
            routingKey: queueName,
            body: body);
    }
}