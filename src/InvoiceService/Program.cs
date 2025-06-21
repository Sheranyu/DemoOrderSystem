using Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory { HostName = "rabbitmq" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
channel.QueueDeclare(queue: "create_order", durable: false, exclusive: false, autoDelete: false, arguments: null);
channel.QueueDeclare(queue: "invoice_created", durable: false, exclusive: false, autoDelete: false, arguments: null);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = JsonSerializer.Deserialize<CreateOrder>(Encoding.UTF8.GetString(body));
    Console.WriteLine($"Invoice created for order {message?.OrderId}");
    var invoice = new InvoiceCreated(message!.OrderId);
    var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(invoice));
    channel.BasicPublish(exchange: "", routingKey: "invoice_created", basicProperties: null, body: responseBytes);
};
channel.BasicConsume(queue: "create_order", autoAck: true, consumer: consumer);

Console.WriteLine("InvoiceService running...");
Console.ReadLine();
