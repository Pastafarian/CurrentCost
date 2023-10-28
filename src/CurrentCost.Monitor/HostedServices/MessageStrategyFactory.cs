namespace CurrentCost.Monitor.HostedServices
{
    public class MessageStrategyFactory : IMessageStrategyFactory
    {
        private readonly IEnumerable<MessageStrategy> _messageStrategies;

        public MessageStrategyFactory(IEnumerable<MessageStrategy> messageStrategies) => _messageStrategies = messageStrategies;

        public MessageStrategy GetStrategy(string message)
        {
            var strategy = _messageStrategies.FirstOrDefault(x => x.IsMatch(message));

            return strategy ?? new UnknownMessageStrategy();
        }
    }
}
