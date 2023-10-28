namespace CurrentCost.Monitor.HostedServices
{
    public class UnknownMessageStrategy : MessageStrategy
    {
        public override bool IsMatch(string message) => false;

        public override Task ExecuteStrategy(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
