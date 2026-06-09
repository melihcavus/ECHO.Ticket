using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ECHO.Ticket.Business.RabbitMQ;

public class RabbitMQProducer : IMessageProducer
{
    public async Task SendMessageAsync<T>(T message, string queueName)
    {
        // 1. RabbitMQ sunucusuna bağlan
        var factory = new ConnectionFactory { HostName = "localhost" };
        
        // v7 ile gelen yeni Asenkron metotlar (await using ile bellekten otomatik düşer)
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        // 2. Kuyruğu tanımla (QueueDeclareAsync oldu)
        await channel.QueueDeclareAsync(queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // 3. Göndereceğimiz nesneyi JSON formatına çevir
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        // 4. Mesajı kuyruğa fırlat (BasicPublishAsync oldu)
        await channel.BasicPublishAsync(exchange: string.Empty,
            routingKey: queueName,
            body: body);
    }
}