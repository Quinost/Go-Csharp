using API.Shared.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks.Dataflow;

namespace API.Infrastructure.BackgroundServices;

public record JobWrapper(Guid JobId, Func<IServiceProvider, Task> JobAction);

public interface IWorkerpoolService
{
    Task<bool> EnqueueAsync(JobWrapper job, CancellationToken ct = default);
}

public sealed class WorkerpoolService : BackgroundService, IWorkerpoolService
{
    private readonly ActionBlock<JobWrapper> workerPool;
    private readonly IServiceScopeFactory scopeFactory;

    public WorkerpoolService(IServiceScopeFactory scopeFactory, AppConfig config)
    {
        this.scopeFactory = scopeFactory;

        workerPool = new ActionBlock<JobWrapper>(ProcessJobAsync, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = config.WorkersCount,
            BoundedCapacity = config.WorkersCount,
        });
    }

    private async Task ProcessJobAsync(JobWrapper workItem)
    {
        var workerId = Task.CurrentId ?? 0;
        var threadId = Environment.CurrentManagedThreadId;

        Console.WriteLine($"[WP] {workerId} Thread-{threadId} Starting job: {workItem.JobId}");

        try
        {
            using var scope = scopeFactory.CreateScope();
            await workItem.JobAction(scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WP] {workerId} Thread-{threadId} error: {ex.Message}");
        }

        Console.WriteLine($"[WP] {workerId} Thread-{threadId} Finished job: {workItem.JobId}");
    }

    public async Task<bool> EnqueueAsync(JobWrapper job, CancellationToken ct = default) => await workerPool.SendAsync(job, ct);

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        workerPool.Complete();

        try
        {
            workerPool.Completion.Wait(TimeSpan.FromSeconds(30), cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error during WorkerPool shutdown, {0}", ex.Message);
        }

        return base.StopAsync(cancellationToken);
    }
}
