namespace CurrentCost.Monitor.HostedServices
{
    public interface IDataIngestServiceProcessor
    {
        Task Process(CancellationToken stoppingToken);
        public Task StopAsync(CancellationToken cancellationToken);
    }
}
