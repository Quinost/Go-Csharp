namespace API.Shared.Config;

public class AppConfig(ConnectionStringCfg rabbitMq, ConnectionStringCfg databaseSql)
{
    public ConnectionStringCfg RabbitMq { get; } = rabbitMq;
    public ConnectionStringCfg DatabaseSql { get; } = databaseSql;
    public bool Migrate { get; set; } = false;
    public int WorkersCount { get; set; } = 20;
}
