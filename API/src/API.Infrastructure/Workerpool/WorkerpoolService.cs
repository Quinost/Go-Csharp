using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks.Dataflow;

namespace API.Infrastructure.Workerpool;

public record JobWrapper(Guid JobId, Func<IServiceProvider, Task> JobAction);

public sealed class WorkerpoolService : IDisposable
{
    private readonly ActionBlock<JobWrapper> workerPool;
    public WorkerpoolService(IServiceScopeFactory scopeFactory, int workerCount = 20)
    {
        workerPool = new ActionBlock<JobWrapper>(async workItem =>
        {
            var workerId = Task.CurrentId ?? 0;
            var threadId = Environment.CurrentManagedThreadId;

            Console.WriteLine($"[Worker-{workerId}][Thread-{threadId}] Starting job: {workItem.JobId}");
            using var scope = scopeFactory.CreateScope();
            await workItem.JobAction(scope.ServiceProvider);
            Console.WriteLine($"[Worker-{workerId}][Thread-{threadId}] Finished job: {workItem.JobId}");
        }, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = workerCount
        });
    }

    public async Task<bool> EnqueueAsync(JobWrapper job, CancellationToken ct = default) => await workerPool.SendAsync(job, ct);

    public void Dispose()
    {
        workerPool.Complete();
        try
        {
            workerPool.Completion.Wait(TimeSpan.FromSeconds(30));
        }
        catch (Exception ex)
        {
            Console.Write("Error during WorkerPool shutdown, {0}", ex.Message);
        }
    }
}
