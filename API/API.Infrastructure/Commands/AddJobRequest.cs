using API.Mediator.Requests;
using API.Shared;
using API.Shared.Events;

namespace API.Infrastructure.Commands;

public record AddJobRequest(string JobName) : IRequest<Guid>;

internal sealed class AddJobRequestHandler(IPublishEndpoint publishEndpoint) : IRequestHandler<AddJobRequest, Guid>
{
    public async Task<Guid> Handle(AddJobRequest request, CancellationToken cancellationToken = default)
    {
        var guid = Guid.NewGuid();
        var jobEvent = new JobAddedEvent(request.JobName);
        await publishEndpoint.Publish(jobEvent, cancellationToken);
        return guid;
    }
}
