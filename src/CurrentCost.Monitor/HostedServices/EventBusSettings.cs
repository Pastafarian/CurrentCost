using CurrentCost.Infrastructure;
using MassTransit;

namespace CurrentCost.Monitor.HostedServices
{
    public static class EventBusServiceExtensions
    {
        public static IServiceCollection AddEventBusService(this IServiceCollection services, IConfiguration configuration)
        {
            // Get the event bus settings from the configuration
            var settings = configuration.GetSection(nameof(RabbitMqSettings)).Get<RabbitMqSettings>();

            if (settings == null)
            {
                throw new NullReferenceException(
                    "The Event Bus Settings has not been configured. Please check the settings and update them.");
            }

            try
            {
                // Add the event bus to the service collection
                services.AddMassTransit(x =>
                {
                    x.SetKebabCaseEndpointNameFormatter();
                    x.UsingRabbitMq((rabbitContext, rabbitConfig) =>
                    {
                        var address = settings.GetAddress();
                        rabbitConfig.Host(new Uri(address), "/", h =>
                        {
                            h.Username(settings.Username);
                            h.Password(settings.Password);
                        });

                        rabbitConfig.ConfigureEndpoints(rabbitContext);
                        rabbitConfig.Durable = true;
                    });
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception($"amqps://{settings.Host}:{settings.Port}" + settings.Username + " " +
                                    settings.Password + " set" + settings.VirtualHost);
            }

            return services;

        }
    }
}
