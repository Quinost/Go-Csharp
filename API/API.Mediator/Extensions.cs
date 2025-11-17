using API.Mediator.Requests;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace API.Mediator;
public static class Extensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        if (!services.Any(s => s.ServiceType == typeof(IMediator)))
        {
            services.AddSingleton<IMediator, Mediator>();
        }

        RegisterHandlers(services, assembly, typeof(IRequestHandler<,>));
        RegisterHandlers(services, assembly, typeof(ICommandHandler<>));
        RegisterHandlers(services, assembly, typeof(INotificationHandler<>));

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly, Type handlerInterfaceType)
    {
        var handlers = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Select(t => new
            {
                ImplementationType = t,
                ServiceTypes = t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType)
            })
            .Where(x => x.ServiceTypes.Any());

        foreach (var handler in handlers)
        {
            foreach (var serviceType in handler.ServiceTypes)
            {
                services.AddTransient(serviceType, handler.ImplementationType);
            }
        }
    }
}
