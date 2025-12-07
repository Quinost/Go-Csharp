global using MassTransit;

using API.Infrastructure.Consumers;
using API.Infrastructure.Database;
using API.Shared.Config;
using API.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace API.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, AppConfig cfg)
    {
        Mediator.Extensions.AddMediator(services, Assembly.GetExecutingAssembly());

        services.AddDbContext<ApiDbContext>(options => options.UseNpgsql(cfg.DatabaseSql.ConnectionString));

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

                rabbit.ConfigureExchange<JobAddedEvent>();

                rabbit.ConfigureConsumer<JobResultEvent, JobResultConsumer>(context);
            });

        });

        return services;
    }

    public static async Task<IServiceScope> MigrateDb(this IServiceScope scope)
    {
        var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        await db.Database.MigrateAsync();
        return scope;
    }
}

internal static class Bus
{
    private static readonly string fanout = "fanout";
    private static readonly string hashtag = "#";

    extension(IRabbitMqBusFactoryConfigurator bus)
    {
        public void ConfigureConsumer<TEvent, TConsumer>(IBusRegistrationContext context)
            where TConsumer : class, IConsumer
            where TEvent : class, IGoEvent
        {
            bus.ReceiveEndpoint(TEvent.EventName, e =>
            {
                e.ConfigureConsumeTopology = false;
                e.UseRawJsonDeserializer(RawSerializerOptions.All, isDefault: true);

                e.Bind(TEvent.EventName, s =>
                {
                    s.ExchangeType = fanout;
                    s.RoutingKey = hashtag;
                });

                e.ConfigureConsumer<TConsumer>(context);
            });
        }

        public void ConfigureExchange<TEvent>() where TEvent : class, IGoEvent
        {
            bus.Message<TEvent>(topology =>
            {
                topology.SetEntityName(TEvent.EventName);
            });
            bus.Publish<TEvent>(pub =>
            {
                pub.ExchangeType = fanout;
            });
        }
    }
}