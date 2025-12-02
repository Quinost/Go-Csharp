Go workerpool, watermill and C# masstransit, custom mediator. Docker-compose with rabbitmq, aspire, 

## Startup Flow (Aspire Orchestration)

```
RabbitMQ + PostgreSQL (start in parallel)
         ↓
API waits for both to be ready
         ↓
API runs database migrations (--migrate)
         ↓
Go Worker Pool waits for API
         ↓
All services running
```

## Job Processing Flow

### 1. Job Submission
```
Client → API (HTTP POST)
         ↓
API publishes to RabbitMQ queue: job-added
         ↓
Returns acknowledgment to client (job guid)
```

### 2. Job Processing (Go Service)
```
Go Service consumes from: job-added
         ↓
Job picked up by Go worker pool
         ↓
Worker processes job concurrently
         ↓
Go publishes result to RabbitMQ queue: job-result
```

### 3. Result Storage (API Consumer)
```
API Consumer listens on: job-result
         ↓
Receives completed job result
         ↓
Stores result in PostgreSQL
```

## Message Flow
```
          ┌─────────┐
          │ Client  │
          └────┬────┘
               │ HTTP POST /job
               ↓
     ┌────────────────────┐
     │   API (Producer)   │
     └─────────┬──────────┘
               │ publish
               ↓
        ┌──────────────┐
        │  RabbitMQ    │
        │  job-added   │
        └──────┬───────┘
               │ consume
               ↓
       ┌──────────────────┐
       │  Go Worker Pool  │
       │  (Processing)    │
       └────────┬─────────┘
                │ publish
                ↓
         ┌──────────────┐
         │  RabbitMQ    │
         │  job-result  │
         └──────┬───────┘
                │ consume
                ↓
        ┌──────────────────┐
        │  API (Consumer)  │
        └───────┬──────────┘
                │ store
                ↓
         ┌──────────────┐
         │  PostgreSQL  │
         └──────────────┘
```

## RabbitMQ Queues

| Queue Name | Producer | Consumer | Purpose |
|------------|----------|----------|---------|
| `job-added` | API | Go Service | Pending jobs to process |
| `job-result` | Go Service | API | Completed job results |
