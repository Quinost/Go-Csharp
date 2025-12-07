package main

import (
	"context"
	"log"
	"services/internal/config"
	"services/internal/handlers/consumer"
	"services/internal/handlers/publisher"
	router "services/internal/infrastructure/messagerouter"
	"services/pkg/generic"

	"github.com/ThreeDotsLabs/watermill"
)

func main() {
	cfg, err := config.Load("../config.yaml")
	if err != nil {
		log.Fatal(err)
	}

	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()

	workerPool := generic.NewWorkerPool(2)
	workerPool.StartPool(ctx)

	logger := watermill.NewStdLogger(false, false)
	router := router.NewMessageRouter(cfg.RabbitMq.ConnectionString, logger)

	if err := configureConsumersAndPoblishers(router, workerPool); err != nil {
		panic(err)
	}

	go func() {
		if err := router.Run(ctx); err != nil {
			log.Fatal("Router error:", err)
		}
	}()

	<-ctx.Done()
}

func configureConsumersAndPoblishers(router *router.MessageRouter, workerPool *generic.WorkerPool) error {
	jobResultPublisher, err := router.AddPublisher(publisher.JobResultExchangeConfig())
	if err != nil {
		log.Fatal("Failed to add JobResult publisher:", err)
	}

	if err := router.AddConsumerHandler(consumer.NewJobAddedHandler(jobResultPublisher, workerPool)); err != nil {
		log.Fatal("Failed to add JobAddedHandler:", err)
	}

	return nil
}
