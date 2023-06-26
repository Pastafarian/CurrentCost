using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CurrentCost.Messages.Common
{

    public static class EventBusServiceExtensions
    {
        public static IServiceCollection AddEventBusService(this IServiceCollection services, IConfiguration configuration)
        {
            // Get the event bus settings from the configuration
            RabbitMqSettings? settings = configuration.GetSection(nameof(RabbitMqSettings)).Get<RabbitMqSettings>();

            if (settings == null)
            {
                throw new NullReferenceException("The Event Bus Settings has not been configured. Please check the settings and update them.");
            }

            services.AddMassTransit(config => config.UsingRabbitMq((_, cfg) => cfg.Host(settings.Host, x =>
                    {
                        x.Username(settings.Username);
                        x.Password(settings.Password);
                    })));

            return services;
        }
    }
    /// <summary>
    /// Settings file for the EventBus
    /// </summary>
    public class RabbitMqSettings
    {
        /// <summary>
        /// Event Bus Host Address. By default set to "localhost".
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Event Bus Port. By default set to 5672.
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// Event Bus Username. By default set to "guest".
        /// </summary>
        public string Username { get; set; } = "guest";

        /// <summary>
        /// Event Bus Password. By default set to "guest".
        /// </summary>
        public string Password { get; set; } = "guest";

        /// <summary>
        /// Event Bus Virtual Host. By default set to "/".
        /// </summary>
        public string VirtualHost { get; set; } = "/";
    }
}
