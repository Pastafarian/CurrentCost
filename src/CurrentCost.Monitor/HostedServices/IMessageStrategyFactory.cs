namespace CurrentCost.Monitor.HostedServices
{
    public interface IMessageStrategyFactory
    {
        MessageStrategy GetStrategy(string message);
    }
}
