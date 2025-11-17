using API.Mediator.Requests;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace API.Mediator.Wrappers;

internal sealed class CommandHandlerWrapper
{
    private readonly Func<IServiceProvider, ICommand, CancellationToken, Task> handler;

    internal CommandHandlerWrapper(Type commandType)
    {
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(commandType);
        var handleMethod = handlerType.GetMethod(nameof(ICommandHandler<ICommand>.Handle)) ?? throw new InvalidOperationException($"Method 'Handle' not found on handler type {handlerType}");
        var serviceProviderParam = Expression.Parameter(typeof(IServiceProvider), nameof(IServiceProvider));
        var commandParam = Expression.Parameter(typeof(ICommand), "request");
        var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), nameof(CancellationToken));

        var handlerInstance = Expression.Call(typeof(ServiceProviderServiceExtensions),
                                              nameof(ServiceProviderServiceExtensions.GetRequiredService),
                                              Type.EmptyTypes,
                                              serviceProviderParam,
                                              Expression.Constant(handlerType));

        var handleCall = Expression.Call(Expression.Convert(handlerInstance, handlerType),
                                         handleMethod,
                                         Expression.Convert(commandParam, commandType),
                                         cancellationTokenParam);
        var lambda = Expression.Lambda<Func<IServiceProvider, ICommand, CancellationToken, Task>>(handleCall,
                                                                                                serviceProviderParam,
                                                                                                commandParam,
                                                                                                cancellationTokenParam);
        handler = lambda.Compile();
    }

    internal Task Handle(IServiceProvider serviceProvider, ICommand command, CancellationToken cancellationToken) 
        => handler(serviceProvider, command, cancellationToken);
}