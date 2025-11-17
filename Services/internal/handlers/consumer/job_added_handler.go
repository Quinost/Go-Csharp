package consumer

import (
	"context"
	"encoding/json"
	"log"
	"services/internal/constants"
	"services/internal/handlers/publisher"
	router "services/internal/infrastructure/messagerouter"
	"services/pkg/generic"
	"time"

	"github.com/ThreeDotsLabs/watermill/message"
	"github.com/google/uuid"
)

type JobAddedEvent struct {
	JobID        uuid.UUID `json:"jobId"`
	JobName      string    `json:"jobName"`
	CreatedAtUTC time.Time `json:"createdAtUTC"`
}

type JobAddedHandler struct {
	publisher message.Publisher
	pool      *generic.WorkerPool
}

func NewJobAddedHandler(publisher message.Publisher, pool *generic.WorkerPool) *JobAddedHandler {
	return &JobAddedHandler{
		publisher: publisher,
		pool:      pool,
	}
}

func (h *JobAddedHandler) GetQueueConfig() router.QueueConfig {
	return router.QueueConfig{
		QueueName:    constants.JobAddedQueue,
		ExchangeName: constants.JobAddedQueue,
		ExchangeType: constants.ExchangeFanout,
		RoutingKey:   "",
		Durable:      true,
	}
}

func (h *JobAddedHandler) Handle(_ context.Context, msg *message.Message) error {
	var event JobAddedEvent
	if err := json.Unmarshal(msg.Payload, &event); err != nil {
		return err
	}

	h.pool.SubmitNewJob(func() error {
		log.Printf("Processing job: ID=%s, Title=%s", event.JobID, event.JobName)

		result := publisher.JobResultEvent{
			JobID:         uuid.New(),
			JobName:       "completed job",
			CreatedAtUTC:  event.CreatedAtUTC,
			FinishedAtUTC: time.Now(),
		}
		if err := h.publishJobResult(result); err != nil {
			return err
		}
		return nil
	})

	log.Printf("Job: ID=%s, Title=%s added to workerpool", event.JobID, event.JobName)

	return nil
}

func (h *JobAddedHandler) publishJobResult(result publisher.JobResultEvent) error {
	payload, err := json.Marshal(result)
	if err != nil {
		return err
	}

	msg := message.NewMessage(uuid.New().String(), payload)
	msg.Metadata.Set("content-type", "application/json")

	return h.publisher.Publish(constants.JobResultQueue, msg)
}
