using API.Mediator.Requests;
using API.Mediator.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace API.Mediator;

public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> query, CancellationToken cancellationToken = default);
    Task Send(ICommand command, CancellationToken cancellationToken = default);
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification;
}

internal sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private readonly ConcurrentDictionary<Type, IRequestHandlerWrapper> _queryHandlerCache = new();
    private readonly ConcurrentDictionary<Type, CommandHandlerWrapper> _commandHandlerCache = new();
    private readonly ConcurrentDictionary<Type, NotificationHandlerWrapper> _notificationHandlerCache = new();

    public Task Send(ICommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var commandType = command.GetType();
        var wrapper = _commandHandlerCache.GetOrAdd(commandType, static ct => new CommandHandlerWrapper(ct));
        using var scope = serviceProvider.CreateScope();
        return wrapper.Handle(scope.ServiceProvider, command, cancellationToken);
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var commandType = query.GetType();
        var wrapper = _queryHandlerCache.GetOrAdd(commandType, static qt => new RequestHandlerWrapper<TResponse>(qt)) as RequestHandlerWrapper<TResponse>;
        using var scope = serviceProvider.CreateScope();
        return wrapper!.Handle(scope.ServiceProvider, query, cancellationToken);
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);
        var notificationType = notification.GetType();
        var wrapper = _notificationHandlerCache.GetOrAdd(notificationType, static nt => new NotificationHandlerWrapper(nt));
        using var scope = serviceProvider.CreateScope();
        await wrapper.Handle(scope.ServiceProvider, notification, cancellationToken);
    }
}