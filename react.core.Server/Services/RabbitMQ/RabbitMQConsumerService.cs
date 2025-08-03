using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace duoword.admin.Server.Services.RabbitMQ
{
    public class RabbitMQConsumerService : BackgroundService
    {
        private readonly ILogger<RabbitMQConsumerService> _logger;
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IChannel _channel;

        public RabbitMQConsumerService(ILogger<RabbitMQConsumerService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            (_connection, _channel) = await RabbitMQConnectionManager.CreateConnectionAndChannelAsync(_configuration);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation("Received: {Message}", message);

                    await ProcessMessageAsync(message);

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await _channel.BasicConsumeAsync(_configuration["RabbitMQ:QueueName"], false, consumer);
        }

        private async Task ProcessMessageAsync(string message)
        {
            _logger.LogInformation("Processing: {Message}", message);
            await Task.Delay(100); // Simulate work
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            await _channel.CloseAsync();
            await _connection.CloseAsync();
        }
    }
}
