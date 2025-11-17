package generic

import (
	"context"
	"log"
	"sync"
)

type JobFunc func() error

type WorkerPool struct {
	maxWorkers int
	jobQueue   chan JobFunc
	wg         sync.WaitGroup
}

func NewWorkerPool(maxWorkers int) *WorkerPool {
	return &WorkerPool{
		maxWorkers: maxWorkers,
		jobQueue: make(chan JobFunc, maxWorkers * 10),
	}
}

func (p *WorkerPool) StartPool(ctx context.Context) {
	for i := 1; i <= p.maxWorkers; i++ { 
		p.wg.Add(1)
		go p.worker(ctx, i)
	}
}

func (p *WorkerPool) worker(ctx context.Context, id int){
	defer p.wg.Done()
	log.Println("[WP] Worker:", id, "started")

	for {
		select {
		case <- ctx.Done():
			log.Println("[WP] Worker:", id, "stopping")
			return
		case job, ok := <- p.jobQueue:
			if !ok { return }
			if err := job(); err != nil {
				log.Println("[WP] Worker:", id, "error processing job: ", err);
			} else {
				log.Println("[WP] Worker:", id, "job completed")
			}
		}
	}
}

func (p *WorkerPool) SubmitNewJob(job JobFunc) {
	p.jobQueue <- job
}

func (p *WorkerPool) Close() {
	close(p.jobQueue)
	p.wg.Wait()
}