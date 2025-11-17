global using MassTransit;

using API.Infrastructure.Consumers;
using API.Shared.Config;
using API.Shared.Events;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
 
namespace API.Infrastructure;
public static class InfrastructureExtensions
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, AppConfig cfg)
    {
        Mediator.Extensions.AddMediator(services, Assembly.GetExecutingAssembly());

        services.AddMassTransit(bus =>
        {
            bus.AddConsumers(Assembly.GetExecutingAssembly());

            bus.UsingRabbitMq((context, rabbit) =>
            {
                rabbit.Host(cfg.RabbitMq.ConnectionString, h =>
                {
                    h.PublisherConfirmation = false;
                });

                rabbit.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders);


                rabbit.Message<JobAddedEvent>(topology =>
                {
                    topology.SetEntityName("job-added");
                });

                rabbit.Publish<JobAddedEvent>(pub =>
                {
                    pub.ExchangeType = "fanout";
                });

                rabbit.ReceiveEndpoint("job-result", e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.UseRawJsonDeserializer(RawSerializerOptions.All, isDefault: true);

                    e.Bind("job-result", s =>
                    {
                        s.ExchangeType = "fanout";
                        s.RoutingKey = "#";
                    });

                    e.ConfigureConsumer<JobResultConsumer>(context);
                });
            });

        });
        return services;
    }
}