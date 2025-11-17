namespace API.Shared.Config
{
    public class AppConfig
    {
        public AppConfig(RabbitMqCfg rabbitMq)
        {
            RabbitMq = rabbitMq;
        }

        public RabbitMqCfg RabbitMq { get; }
    }
}
