using RabbitMQ.Client;


namespace duoword.admin.Server.Services
{
    public class RabbitMQService : IAsyncDisposable
    {
        private readonly ConnectionFactory _factory;
        private Task<IConnection>? _connectionTask;

        public RabbitMQService(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("RabbitMQ") ?? "";

            _factory = new ConnectionFactory
            {
                Uri = new Uri(connectionString)
            };
        }

        private async Task<IConnection> GetConnectionAsync()
        {
            if (_connectionTask == null || !(await _connectionTask).IsOpen)
            {
                _connectionTask = _factory.CreateConnectionAsync();
            }
            return await _connectionTask;
        }

        public async Task<IChannel> GetChannelAsync()
        {
            var connection = await GetConnectionAsync();
            return await connection.CreateChannelAsync(); // Always return a fresh channel
        }

        public async ValueTask DisposeAsync()
        {
            if (_connectionTask != null)
            {
                var connection = await _connectionTask;
                if (connection.IsOpen)
                {
                    await connection.CloseAsync();
                }
            }
        }
    }

}

