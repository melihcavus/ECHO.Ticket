namespace ECHO.Ticket.Business.RabbitMQ;

public interface IMessageProducer
{
    Task SendMessageAsync<T>(T message, string queueName);
}