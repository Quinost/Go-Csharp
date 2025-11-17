using API.Mediator.Requests;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace API.Mediator.Wrappers;
internal interface IRequestHandlerWrapper { }

internal sealed class RequestHandlerWrapper<TResponse> : IRequestHandlerWrapper
{
    private readonly Func<IServiceProvider, IRequest<TResponse>, CancellationToken, Task<TResponse>> handler;

    internal RequestHandlerWrapper(Type requestType)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle)) ?? throw new InvalidOperationException($"Method 'Handle' not found on handler type {handlerType}");

        var serviceProviderParam = Expression.Parameter(typeof(IServiceProvider), nameof(IServiceProvider));
        var requestParam = Expression.Parameter(typeof(IRequest<TResponse>), "request");
        var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), nameof(CancellationToken));

        var handlerInstance = Expression.Call(typeof(ServiceProviderServiceExtensions),
                                      nameof(ServiceProviderServiceExtensions.GetRequiredService),
                                      Type.EmptyTypes,
                                      serviceProviderParam,
                                      Expression.Constant(handlerType));

        var handleCall = Expression.Call(Expression.Convert(handlerInstance, handlerType),
                                         handleMethod,
                                         Expression.Convert(requestParam, requestType),
                                         cancellationTokenParam);

        var lambda = Expression.Lambda<Func<IServiceProvider, IRequest<TResponse>, CancellationToken, Task<TResponse>>>(handleCall,
                                                                                                                      serviceProviderParam,
                                                                                                                      requestParam,
                                                                                                                      cancellationTokenParam);

        handler = lambda.Compile();
    }

    internal Task<TResponse> Handle(IServiceProvider serviceProvider, IRequest<TResponse> request, CancellationToken cancellationToken) 
        => handler(serviceProvider, request, cancellationToken);
}