package job_models

import (
	"time"

	"github.com/google/uuid"
)

type JobStatus string

const (
	Pending    JobStatus = "Pending"
	InProgress JobStatus = "InProgress"
	Completed  JobStatus = "Completed"
	Failed     JobStatus = "Failed"
)

type JobAddedEvent struct {
	JobID        uuid.UUID `json:"jobId"`
	Name         string    `json:"name"`
	Status       JobStatus `json:"status"`
	CreatedAtUTC time.Time `json:"createdAtUTC"`
}

type JobResultEvent struct {
	JobID         uuid.UUID `json:"jobId"`
	Name          string    `json:"name"`
	Status        JobStatus `json:"status"`
	Reason        string    `json:"reason,omitempty"`
	CreatedAtUTC  time.Time `json:"createdAtUTC"`
	FinishedAtUTC time.Time `json:"finishedAtUTC"`
}
