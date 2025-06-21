using Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/orders", (CreateOrder order) =>
{
    var factory = new ConnectionFactory { HostName = "rabbitmq" };
    using var connection = factory.CreateConnection();
    using var channel = connection.CreateModel();
    channel.QueueDeclare(queue: "create_order", durable: false, exclusive: false, autoDelete: false, arguments: null);
    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(order));
    channel.BasicPublish(exchange: "", routingKey: "create_order", basicProperties: null, body: body);
    return Results.Accepted($"/orders/{order.OrderId}");
});

app.Run("http://0.0.0.0:5000");
