package router

import (
	"context"

	"github.com/ThreeDotsLabs/watermill/message"
)

type QueueConfig struct {
	QueueName    string
	ExchangeName string
	ExchangeType string
	RoutingKey   string
	Durable      bool
}

type ExchangeConfig struct {
	ExchangeName string
	ExchangeType string
	RoutingKey   string
	Durable      bool
}

type MessageHandler interface {
	Handle(ctx context.Context, msg *message.Message) error
	GetQueueConfig() QueueConfig
}