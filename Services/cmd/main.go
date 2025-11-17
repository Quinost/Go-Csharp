package main

import (
	"context"
	"log"
	"services/internal/handlers/consumer"
	"services/internal/handlers/publisher"
	router "services/internal/infrastructure/messagerouter"
	"services/pkg/generic"

	"github.com/ThreeDotsLabs/watermill"
)

func main() {
	rabbitURL := "amqp://guest:guest@localhost:5672/"
	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()

	workerPool := generic.NewWorkerPool(10)
	workerPool.StartPool(ctx)

	logger := watermill.NewStdLogger(false, false)
	router := router.NewMessageRouter(rabbitURL, logger)

	jobResultPublisher, err := router.AddPublisher(publisher.GetJobResultExchangeConfig())
	if err != nil {
		log.Fatal("Failed to add JobResult publisher:", err)
	}

	if err := router.AddConsumerHandler(consumer.NewJobAddedHandler(jobResultPublisher, workerPool)); err != nil {
		log.Fatal("Failed to add JobAddedHandler:", err)
	}

	go func() {
		if err := router.Run(ctx); err != nil {
			log.Fatal("Router error:", err)
		}
	}()

	<-ctx.Done()
}
