package models

type Config struct {
	RabbitMq ConnectionCfg `yaml:"rabbitmq"`
}

type ConnectionCfg struct {
	ConnectionString string `yaml:"connectionString"`
}