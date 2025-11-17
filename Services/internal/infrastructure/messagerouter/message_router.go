package router

import (
	"context"
	"log"

	"github.com/ThreeDotsLabs/watermill"
	"github.com/ThreeDotsLabs/watermill-amqp/v2/pkg/amqp"
	"github.com/ThreeDotsLabs/watermill/message"
)

type MessageRouter struct {
	router      *message.Router
	handlers    []MessageHandler
	subscribers map[string]message.Subscriber
	publishers  map[string]message.Publisher
	logger      watermill.LoggerAdapter
	rabbitURL   string
}

func NewMessageRouter(rabbitURL string, logger watermill.LoggerAdapter) *MessageRouter {
	router, err := message.NewRouter(message.RouterConfig{}, logger)
	if err != nil {
		log.Fatal(err)
	}

	return &MessageRouter{
		router:      router,
		handlers:    []MessageHandler{},
		subscribers: make(map[string]message.Subscriber),
		publishers:  make(map[string]message.Publisher),
		logger:      logger,
		rabbitURL:   rabbitURL,
	}
}


func (mr *MessageRouter) Run(ctx context.Context) error {
	return mr.router.Run(ctx)
}

func (mr *MessageRouter) Close() error {
	for name, sub := range mr.subscribers {
		log.Printf("Closing subscriber for: %s", name)
		if err := sub.Close(); err != nil {
			log.Printf("Error closing subscriber %s: %v", name, err)
		}
	}
	
	for name, pub := range mr.publishers {
		log.Printf("Closing publisher for: %s", name)
		if err := pub.Close(); err != nil {
			log.Printf("Error closing publisher %s: %v", name, err)
		}
	}
	
	return mr.router.Close()
}

// Add consumer (only queue and bind without exchange)
func (mr *MessageRouter) AddConsumerHandler(handler MessageHandler) error {
	mr.handlers = append(mr.handlers, handler)
	
	config := handler.GetQueueConfig()
	
	subscriberConfig := createSubscriberConfigForQueue(mr.rabbitURL, config)
	subscriber, err := amqp.NewSubscriber(subscriberConfig, mr.logger)
	if err != nil {
		return err
	}
	
	mr.subscribers[config.QueueName] = subscriber

	mr.router.AddConsumerHandler(
		config.QueueName+"_handler",
		config.ExchangeName,
		subscriber,
		func(msg *message.Message) error {
			return handler.Handle(context.Background(), msg)
		},
	)
	
	log.Printf("Registered consumer handler for queue: %s, exchange: %s", config.QueueName, config.ExchangeName)
	return nil
}

// Add publisher (only exchange without queue and binding)
func (mr *MessageRouter) AddPublisher(exchangeConfig ExchangeConfig) (message.Publisher, error) {
	if pub, exists := mr.publishers[exchangeConfig.ExchangeName]; exists {
		return pub, nil
	}

	publisherConfig := createPublisherConfigForExchange(mr.rabbitURL, exchangeConfig)
	publisher, err := amqp.NewPublisher(publisherConfig, mr.logger)
	if err != nil {
		return nil, err
	}

	mr.publishers[exchangeConfig.ExchangeName] = publisher
	log.Printf("Registered publisher for exchange: %s (type: %s)", exchangeConfig.ExchangeName, exchangeConfig.ExchangeType)
	
	return publisher, nil
}

// createSubscriberConfigForQueue tworzy konfigurację subscribera na podstawie QueueConfig
func createSubscriberConfigForQueue(rabbitURL string, qConfig QueueConfig) amqp.Config {
	config := amqp.NewDurableQueueConfig(rabbitURL)

	config.Exchange = amqp.ExchangeConfig{
		GenerateName: func(topic string) string {
			return qConfig.ExchangeName
		},
		Type:    qConfig.ExchangeType,
		Durable: qConfig.Durable,
	}

	config.Queue = amqp.QueueConfig{
		GenerateName: func(topic string) string {
			return qConfig.QueueName
		},
		Durable: qConfig.Durable,
	}

	config.QueueBind = amqp.QueueBindConfig{
		GenerateRoutingKey: func(topic string) string {
			if qConfig.RoutingKey == "" {
				return "#"
			}
			return qConfig.RoutingKey
		},
	}

	return config
}

func createPublisherConfigForExchange(rabbitURL string, eConfig ExchangeConfig) amqp.Config {
	config := amqp.Config{
		Connection: amqp.ConnectionConfig{
			AmqpURI: rabbitURL,
		},
		Marshaler: amqp.DefaultMarshaler{},
		TopologyBuilder: &amqp.DefaultTopologyBuilder{},
		Publish: amqp.PublishConfig{
			ChannelPoolSize: 20,
		},
	}

	config.Exchange = amqp.ExchangeConfig{
		GenerateName: func(topic string) string {
			return eConfig.ExchangeName
		},
		Type:    eConfig.ExchangeType,
		Durable: eConfig.Durable,
	}

	config.Publish = amqp.PublishConfig{
		GenerateRoutingKey: func(topic string) string {
			return eConfig.RoutingKey
		},
	}

	return config
}