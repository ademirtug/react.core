using Microsoft.AspNetCore.Connections;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace duoword.admin.Server.Services.RabbitMQ
{
    public interface IRabbitMQPublisher
    {
        Task PublishMessageAsync<T>(T message);
    }

    public class RabbitMQPublisher : IRabbitMQPublisher, IAsyncDisposable
    {
        private readonly IChannel _channel;
        private readonly string _exchangeName;
        private readonly string _routingKey;
        private readonly IConnection _connection;

        public RabbitMQPublisher(IConfiguration config)
        {
            var (conn, channel) = RabbitMQConnectionManager.CreateConnectionAndChannelAsync(config).Result;
            _connection = conn;
            _channel = channel;

            _exchangeName = config["RabbitMQ:ExchangeName"];
            _routingKey = config["RabbitMQ:RoutingKey"];
        }

        public async Task PublishMessageAsync<T>(T message)
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            var props = new BasicProperties { Persistent = true };

            await _channel.BasicPublishAsync(_exchangeName, _routingKey, false, props, body, CancellationToken.None);
        }

        public async ValueTask DisposeAsync()
        {
            await _channel.CloseAsync();
            await _connection.CloseAsync();
        }
    }

    public class RabbitMQConnectionManager
    {
        public static async Task<(IConnection, IChannel)> CreateConnectionAndChannelAsync(IConfiguration config)
        {
            var factory = new ConnectionFactory
            {
                HostName = config["RabbitMQ:HostName"],
                Port = int.Parse(config["RabbitMQ:Port"]),
                UserName = config["RabbitMQ:UserName"],
                Password = config["RabbitMQ:Password"],
            };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            string exchangeName = config["RabbitMQ:ExchangeName"];
            string queueName = config["RabbitMQ:QueueName"];
            string routingKey = config["RabbitMQ:RoutingKey"];

            await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, durable: true);
            await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);
            await channel.QueueBindAsync(queueName, exchangeName, routingKey);

            return (connection, channel);
        }
    }
}
