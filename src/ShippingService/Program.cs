using Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory { HostName = "rabbitmq" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
channel.QueueDeclare(queue: "payment_booked", durable: false, exclusive: false, autoDelete: false, arguments: null);
channel.QueueDeclare(queue: "order_shipped", durable: false, exclusive: false, autoDelete: false, arguments: null);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = JsonSerializer.Deserialize<PaymentBooked>(Encoding.UTF8.GetString(body));
    Console.WriteLine($"Order shipped {message?.OrderId}");
    var shipped = new OrderShipped(message!.OrderId);
    var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(shipped));
    channel.BasicPublish(exchange: "", routingKey: "order_shipped", basicProperties: null, body: responseBytes);
};
channel.BasicConsume(queue: "payment_booked", autoAck: true, consumer: consumer);

Console.WriteLine("ShippingService running...");
Console.ReadLine();
