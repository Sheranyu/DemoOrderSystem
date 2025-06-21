using Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory { HostName = "rabbitmq" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
channel.QueueDeclare(queue: "invoice_created", durable: false, exclusive: false, autoDelete: false, arguments: null);
channel.QueueDeclare(queue: "payment_booked", durable: false, exclusive: false, autoDelete: false, arguments: null);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = JsonSerializer.Deserialize<InvoiceCreated>(Encoding.UTF8.GetString(body));
    Console.WriteLine($"Payment booked for order {message?.OrderId}");
    var payment = new PaymentBooked(message!.OrderId);
    var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payment));
    channel.BasicPublish(exchange: "", routingKey: "payment_booked", basicProperties: null, body: responseBytes);
};
channel.BasicConsume(queue: "invoice_created", autoAck: true, consumer: consumer);

Console.WriteLine("PaymentService running...");
Console.ReadLine();
