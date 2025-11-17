package publisher

import (
	"services/internal/constants"
	router "services/internal/infrastructure/messagerouter"
	"time"

	"github.com/google/uuid"
)

type JobResultEvent struct {
	JobID         uuid.UUID `json:"jobId"`
	JobName       string    `json:"jobName"`
	CreatedAtUTC  time.Time `json:"createdAtUTC"`
	FinishedAtUTC time.Time `json:"finishedAtUTC"`
}

func GetJobResultExchangeConfig() router.ExchangeConfig {
	return router.ExchangeConfig{
		ExchangeName: constants.JobResultQueue,
		ExchangeType: constants.ExchangeFanout,
		RoutingKey:   "",
		Durable:      true,
	}
}
