using API.Mediator.Requests;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace API.Mediator.Wrappers;
internal sealed class NotificationHandlerWrapper
{
    private readonly Func<IServiceProvider, INotification, CancellationToken, Task> handler;

    internal NotificationHandlerWrapper(Type notificationType)
    {
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
        var handleMethod = handlerType.GetMethod(nameof(INotificationHandler<INotification>.Handle)) ?? throw new InvalidOperationException($"Method 'Handle' not found on handler type {handlerType}");

        var serviceProviderParam = Expression.Parameter(typeof(IServiceProvider), nameof(IServiceProvider));
        var notificationParam = Expression.Parameter(typeof(INotification), "request");
        var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), nameof(CancellationToken));

        var getServicesMethod = typeof(ServiceProviderServiceExtensions)
            .GetMethod(nameof(ServiceProviderServiceExtensions.GetServices), [typeof(IServiceProvider)])!
            .MakeGenericMethod(handlerType);
        var getServicesCall = Expression.Call(getServicesMethod, serviceProviderParam);

        var handleParam = Expression.Parameter(handlerType, nameof(handlerType));
        var handleCall = Expression.Call(handleParam,
                                         handleMethod,
                                         Expression.Convert(notificationParam, notificationType),
                                         cancellationTokenParam);
        var handlerLambda = Expression.Lambda(handleCall, handleParam);
        var selectMethods = typeof(Enumerable)
                                              .GetMethods()
                                              .First(m => m.Name == nameof(Enumerable.Select) && m.GetParameters().Length == 2)
                                              .MakeGenericMethod(handlerType, typeof(Task));
        var tasksExpression = Expression.Call(selectMethods, getServicesCall, handlerLambda);

        var toArrayMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))!.MakeGenericMethod(typeof(Task));
        var tasksArray = Expression.Call(toArrayMethod, tasksExpression);
        var whenAllMethod = typeof(Task).GetMethod(nameof(Task.WhenAll), [typeof(Task[])])!;
        var whenAllCall = Expression.Call(whenAllMethod, tasksArray);

        handler = Expression.Lambda<Func<IServiceProvider, INotification, CancellationToken, Task>>(
            whenAllCall, serviceProviderParam, notificationParam, cancellationTokenParam).Compile();
    }

    internal Task Handle(IServiceProvider serviceProvider, INotification notification, CancellationToken cancellationToken) 
        => handler(serviceProvider, notification, cancellationToken);
}
