using API.Shared.Events;

namespace API.Infrastructure.Consumers;

public class JobResultConsumer : IConsumer<JobResultEvent>
{
    public Task Consume(ConsumeContext<JobResultEvent> context)
    {
        var ddd = context.Message;
        Console.WriteLine(ddd);
        return Task.CompletedTask;
    }
}
