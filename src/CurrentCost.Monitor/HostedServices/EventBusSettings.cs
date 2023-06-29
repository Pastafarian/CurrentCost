using System.Text;
using CurrentCost.Consumers;
using CurrentCost.Messages.Common;
using MassTransit;
using RabbitMQ.Client;

namespace CurrentCost.Monitor.HostedServices
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

            try
            {
                // Add the event bus to the service collection
                //services.AddMassTransit(x =>
                //{
                //    //x.SetKebabCaseEndpointNameFormatter();
                //    //x.AddConsumer(typeof(MonitorMessageConsumer));
                //    //x.UsingRabbitMq((rabbitContext, rabbitConfig) =>
                //    //{
                //    //    rabbitConfig.Host(new Uri($"amqp://rabbit-mq:5672"), "/", h =>
                //    //    {
                //    //        h.Username("guest");
                //    //        h.Password("guest");
                //    //    });

                //    //    rabbitConfig.ConfigureEndpoints(rabbitContext);
                //    //    rabbitConfig.Durable = true;
                //    //});
                //    x.SetKebabCaseEndpointNameFormatter();
                //    x.AddConsumer(typeof(MonitorMessageConsumer));
                //    //x.UsingRabbitMq((rabbitContext, rabbitConfig) =>
                //    //{
                //    //    rabbitConfig.Host(new Uri($"amqp://{settings.Host}:{settings.Port}"), "/", h =>
                //    //    {
                //    //        h.Username(settings.Username);
                //    //        h.Password(settings.Password);
                //    //    });
                //    //    rabbitConfig.ConfigureEndpoints(rabbitContext);
                //    //    rabbitConfig.Durable = true;
                //    //});
                //});
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception($"amqps://{settings.Host}:{settings.Port}" + settings.Username + " " + settings.Password + " set" + settings.VirtualHost);
            }
         //   MessageService messageService = new MessageService();



            return services;
        }

    }
    public class MessageService {
    ConnectionFactory _factory;
    IConnection _conn;
    IModel _channel;

    public MessageService()
    {
        Console.WriteLine("about to connect to rabbit");

        _factory = new ConnectionFactory() { HostName ="rabbit-mq", Port = 5672 };
        _factory.UserName = "guest";
        _factory.Password = "guest";
        _conn = _factory.CreateConnection();
        _channel = _conn.CreateModel();
        _channel.QueueDeclare(queue: "hello",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }
    public bool Enqueue(string messageString)
    {
        var body = Encoding.UTF8.GetBytes("server processed " + messageString);
        _channel.BasicPublish(exchange: "",
            routingKey: "hello",
            basicProperties: null,
            body: body);
        Console.WriteLine(" [x] Published {0} to RabbitMQ", messageString);
        return true;
    }

}
}
