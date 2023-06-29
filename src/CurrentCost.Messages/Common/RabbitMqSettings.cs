namespace CurrentCost.Messages.Common
{
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
