package config

import (
	"os"
	"services/pkg/models"

	"gopkg.in/yaml.v2"
)

func Load(path string) (*models.Config, error) {
	f, err := os.Open(path)

	if err != nil {
		return nil, err
	}

	defer f.Close()

	var cfg models.Config
	decoder := yaml.NewDecoder(f)
	err = decoder.Decode(&cfg)

	if rabbitMqUrl := os.Getenv("RabbitMq__ConnectionString"); rabbitMqUrl != "" {
		cfg.RabbitMq.ConnectionString = rabbitMqUrl
	}

	return &cfg, err
}
