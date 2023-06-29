using System.Xml.Serialization;
using CurrentCost.Messages.Messages;
using CurrentCost.Monitor.HostedServices;

namespace CurrentCost.Monitor.Infrastructure.Deserialization
{
    public class MonitorMessageDeserializer : IMonitorMessageDeserializer
    {
        private readonly ILogger<MonitorMessageDeserializer> _logger;

        public MonitorMessageDeserializer(ILogger<MonitorMessageDeserializer> logger) => _logger = logger;

        public MonitorMessage Deserialize(string messageXml)
        {
            messageXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" + messageXml;

            var serializer = new XmlSerializer(typeof(MonitorMessage));
            using TextReader reader = new StringReader(messageXml);
            try
            {
                if (serializer.Deserialize(reader) is not MonitorMessage message)
                {
                    _logger.LogError("Failed to deserialize message: {message} into {type}", messageXml, typeof(MonitorMessage));
                    return null;
                }
                

                return message;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to deserialize message: {message} into {type}", messageXml, typeof(MonitorMessage));
            }
            return null;
        }
    }
}
