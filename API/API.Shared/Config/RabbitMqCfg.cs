namespace API.Shared.Config
{
    public sealed class RabbitMqCfg
    {
        public RabbitMqCfg(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }
    }
}
