using API.Infrastructure.Database;
using API.Infrastructure.Workerpool;
using API.Shared.Entities;
using API.Shared.Events;
using Microsoft.Extensions.DependencyInjection;

namespace API.Infrastructure.Consumers;

public class JobResultConsumer(WorkerpoolService workerpoolService) : IConsumer<JobResultEvent>
{
    public async Task Consume(ConsumeContext<JobResultEvent> context)
    {
        var job = context.Message;

        var jobWrapper = new JobWrapper(job.JobId, async (sp) =>
        {
            var dbContext = sp.GetRequiredService<ApiDbContext>();

            var result = new JobResult()
            {
                JobId = job.JobId,
                Name = job.Name,
                Status = job.Status.ToString(),
                Reason = job.Reason,
                CreatedAt = job.CreatedAtUTC,
                FinishedAt = job.FinishedAtUTC,
            };

            Thread.Sleep(TimeSpan.FromSeconds(10));

            await dbContext.JobResults.AddAsync(result);
            await dbContext.SaveChangesAsync();
        });

        await workerpoolService.EnqueueAsync(jobWrapper);
    }
}
